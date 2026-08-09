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
            if (HasAccessibilityControls(snapshot))
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
        Queue<(AutomationElement Element, int Depth, int ParentIndex)> pending = new();
        List<UiaElementSnapshot> elements = [];
        pending.Enqueue((root, 0, -1));

        while (pending.TryDequeue(out (AutomationElement Element, int Depth, int ParentIndex) current)
            && elements.Count < MaximumElements)
        {
            try
            {
                AutomationElement.AutomationElementInformation information = current.Element.Current;
                int elementIndex = elements.Count;
                int[] runtimeId = current.Element.GetRuntimeId()
                    ?? throw new InvalidOperationException("A UI Automation element returned a null runtime ID.");
                string[] supportedPatterns = current.Element.GetSupportedPatterns()
                    .Select(pattern => pattern.ProgrammaticName
                        ?? throw new InvalidOperationException("A UI Automation pattern had no programmatic name."))
                    .OrderBy(pattern => pattern, StringComparer.Ordinal)
                    .ToArray();
                elements.Add(new(
                    current.Depth,
                    information.Name ?? string.Empty,
                    information.AutomationId ?? string.Empty,
                    information.ControlType?.ProgrammaticName ?? string.Empty,
                    information.NativeWindowHandle,
                    information.IsKeyboardFocusable,
                    information.HasKeyboardFocus,
                    current.ParentIndex,
                    string.Join('.', runtimeId),
                    supportedPatterns));

                if (current.Depth >= MaximumDepth)
                {
                    continue;
                }

                AutomationElement? child = walker.GetFirstChild(current.Element);
                while (child is not null && pending.Count + elements.Count < MaximumElements)
                {
                    pending.Enqueue((child, current.Depth + 1, elementIndex));
                    child = walker.GetNextSibling(child);
                }
            }
            catch (ElementNotAvailableException)
            {
            }
        }

        return new(rootWindowHandle, elements);
    }

    private static bool HasAccessibilityControls(UiaSnapshot snapshot)
    {
        bool hasColorPicker = false;
        bool hasFocusedAction = false;
        bool hasRange = false;
        bool hasValue = false;

        foreach (UiaElementSnapshot element in snapshot.Elements)
        {
            hasColorPicker |= element.AutomationId == UiaAccessibilityAssertions.ColorPickerAutomationId;
            hasFocusedAction |= element.AutomationId == UiaAccessibilityAssertions.ActionAutomationId
                && element.HasKeyboardFocus;
            hasRange |= element.AutomationId == UiaAccessibilityAssertions.RangeAutomationId;
            hasValue |= element.AutomationId == UiaAccessibilityAssertions.ValueAutomationId;
        }

        return hasColorPicker && hasFocusedAction && hasRange && hasValue;
    }
}
