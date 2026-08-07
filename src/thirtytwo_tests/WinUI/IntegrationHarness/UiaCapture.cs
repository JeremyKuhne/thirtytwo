// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows.Automation;

namespace Windows.WinUI.IntegrationHarness;

internal static class UiaCapture
{
    private const int MaximumDepth = 32;
    private const int MaximumElements = 512;

    internal static async Task<UiaSnapshot> CaptureAsync(
        long rootWindowHandle,
        int expectedProcessId,
        TimeSpan timeout)
    {
        if (rootWindowHandle == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rootWindowHandle));
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        UiaSnapshot snapshot;
        do
        {
            WindowHandleValidation.Validate(rootWindowHandle, expectedProcessId);
            snapshot = Capture(rootWindowHandle);
            if (HasColorPickerControls(snapshot))
            {
                return snapshot;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
        }
        while (stopwatch.Elapsed < timeout);

        return snapshot;
    }

    private static UiaSnapshot Capture(long rootWindowHandle)
    {
        AutomationElement root = AutomationElement.FromHandle((nint)rootWindowHandle)
            ?? throw new InvalidOperationException("UI Automation did not return a root element.");
        TreeWalker walker = TreeWalker.ControlViewWalker;
        Queue<(AutomationElement Element, int Depth)> pending = new();
        List<UiaElementSnapshot> elements = [];
        pending.Enqueue((root, 0));

        while (pending.TryDequeue(out (AutomationElement Element, int Depth) current)
            && elements.Count < MaximumElements)
        {
            try
            {
                AutomationElement.AutomationElementInformation information = current.Element.Current;
                elements.Add(new(
                    current.Depth,
                    information.Name ?? string.Empty,
                    information.AutomationId ?? string.Empty,
                    information.ControlType?.ProgrammaticName ?? string.Empty,
                    information.NativeWindowHandle,
                    information.IsKeyboardFocusable,
                    information.HasKeyboardFocus));

                if (current.Depth >= MaximumDepth)
                {
                    continue;
                }

                AutomationElement? child = walker.GetFirstChild(current.Element);
                while (child is not null && pending.Count + elements.Count < MaximumElements)
                {
                    pending.Enqueue((child, current.Depth + 1));
                    child = walker.GetNextSibling(child);
                }
            }
            catch (ElementNotAvailableException)
            {
            }
        }

        return new(rootWindowHandle, elements);
    }

    private static bool HasColorPickerControls(UiaSnapshot snapshot)
    {
        bool hasSlider = false;
        bool hasComboBox = false;
        bool hasEdit = false;

        foreach (UiaElementSnapshot element in snapshot.Elements)
        {
            hasSlider |= element.ControlType == ControlType.Slider.ProgrammaticName;
            hasComboBox |= element.ControlType == ControlType.ComboBox.ProgrammaticName;
            hasEdit |= element.ControlType == ControlType.Edit.ProgrammaticName;
        }

        return hasSlider && hasComboBox && hasEdit;
    }
}
