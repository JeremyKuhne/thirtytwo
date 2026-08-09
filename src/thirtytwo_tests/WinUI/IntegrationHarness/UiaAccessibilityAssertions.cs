// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows.Automation;

namespace Windows.WinUI.IntegrationHarness;

internal static class UiaAccessibilityAssertions
{
    internal const string RootAutomationId = "AccessibilityRoot";
    internal const string ColorPickerAutomationId = "AccessibilityColorPicker";
    internal const string ActionAutomationId = "AccessibilityAction";
    internal const string RangeAutomationId = "AccessibilityRange";
    internal const string ValueAutomationId = "AccessibilityValue";

    internal static void Assert(UiaSnapshot snapshot, long rootWindowHandle)
    {
        snapshot.RootWindowHandle.Should().Be(rootWindowHandle);
        snapshot.Elements.Should().NotBeEmpty();
        snapshot.Elements.Count(element => element.ParentIndex == -1).Should().Be(1);

        UiaElementSnapshot root = snapshot.Elements[0];
        root.ParentIndex.Should().Be(-1);
        root.Depth.Should().Be(0);
        int rootNativeWindowHandle = unchecked((int)rootWindowHandle);
        root.NativeWindowHandle.Should().Be(rootNativeWindowHandle);

        for (int index = 1; index < snapshot.Elements.Count; index++)
        {
            UiaElementSnapshot element = snapshot.Elements[index];
            element.ParentIndex.Should().BeInRange(0, index - 1, $"element {index} should reference an earlier parent");
            element.Depth.Should().Be(
                snapshot.Elements[element.ParentIndex].Depth + 1,
                $"element {index} should be exactly one level below its parent");
        }

        snapshot.Elements.Select(element => element.RuntimeId).Should().NotContain(string.Empty);
        snapshot.Elements.Select(element => element.RuntimeId).Should().OnlyHaveUniqueItems();

        int accessibilityRootIndex = GetSingleIndex(snapshot, RootAutomationId);
        UiaElementSnapshot accessibilityRoot = snapshot.Elements[accessibilityRootIndex];
        accessibilityRoot.ControlType.Should().Be(ControlType.Group.ProgrammaticName);
        HasDistinctNativeAncestor(snapshot, accessibilityRootIndex, rootNativeWindowHandle).Should().BeTrue();

        UiaElementSnapshot colorPicker = GetSingle(snapshot, ColorPickerAutomationId);
        colorPicker.ParentIndex.Should().Be(accessibilityRootIndex);
        colorPicker.ControlType.Should().Be(ControlType.Group.ProgrammaticName);
        int colorPickerIndex = GetSingleIndex(snapshot, ColorPickerAutomationId);

        UiaElementSnapshot colorPickerSlider = GetFirstDescendant(
            snapshot,
            colorPickerIndex,
            element => element.ControlType == ControlType.Slider.ProgrammaticName
                && element.SupportedPatterns.Contains(RangeValuePatternIdentifiers.Pattern.ProgrammaticName));
        colorPickerSlider.SupportedPatterns.Should().Contain(ValuePatternIdentifiers.Pattern.ProgrammaticName);

        UiaElementSnapshot colorPickerComboBox = GetFirstDescendant(
            snapshot,
            colorPickerIndex,
            element => element.ControlType == ControlType.ComboBox.ProgrammaticName);
        colorPickerComboBox.SupportedPatterns.Should().Contain(ExpandCollapsePatternIdentifiers.Pattern.ProgrammaticName);
        colorPickerComboBox.SupportedPatterns.Should().Contain(SelectionPatternIdentifiers.Pattern.ProgrammaticName);

        UiaElementSnapshot colorPickerEdit = GetFirstDescendant(
            snapshot,
            colorPickerIndex,
            element => element.ControlType == ControlType.Edit.ProgrammaticName);
        colorPickerEdit.SupportedPatterns.Should().Contain(ValuePatternIdentifiers.Pattern.ProgrammaticName);
        colorPickerEdit.SupportedPatterns.Should().Contain(TextPatternIdentifiers.Pattern.ProgrammaticName);

        UiaElementSnapshot action = GetSingle(snapshot, ActionAutomationId);
        action.ParentIndex.Should().Be(accessibilityRootIndex);
        action.ControlType.Should().Be(ControlType.Button.ProgrammaticName);
        action.HasKeyboardFocus.Should().BeTrue();
        action.SupportedPatterns.Should().Contain(InvokePatternIdentifiers.Pattern.ProgrammaticName);

        UiaElementSnapshot range = GetSingle(snapshot, RangeAutomationId);
        range.ParentIndex.Should().Be(accessibilityRootIndex);
        range.ControlType.Should().Be(ControlType.Slider.ProgrammaticName);
        range.SupportedPatterns.Should().Contain(RangeValuePatternIdentifiers.Pattern.ProgrammaticName);

        UiaElementSnapshot value = GetSingle(snapshot, ValueAutomationId);
        value.ParentIndex.Should().Be(accessibilityRootIndex);
        value.ControlType.Should().Be(ControlType.Edit.ProgrammaticName);
        value.SupportedPatterns.Should().Contain(ValuePatternIdentifiers.Pattern.ProgrammaticName);
        value.SupportedPatterns.Should().Contain(TextPatternIdentifiers.Pattern.ProgrammaticName);
    }

    private static UiaElementSnapshot GetSingle(UiaSnapshot snapshot, string automationId)
        => snapshot.Elements.Single(element => element.AutomationId == automationId);

    private static int GetSingleIndex(UiaSnapshot snapshot, string automationId)
    {
        int foundIndex = -1;
        for (int index = 0; index < snapshot.Elements.Count; index++)
        {
            if (snapshot.Elements[index].AutomationId != automationId)
            {
                continue;
            }

            foundIndex.Should().Be(-1, $"automation id '{automationId}' should occur exactly once");
            foundIndex = index;
        }

        foundIndex.Should().BeGreaterThanOrEqualTo(0, $"automation id '{automationId}' should be present");
        return foundIndex;
    }

    private static UiaElementSnapshot GetFirstDescendant(
        UiaSnapshot snapshot,
        int ancestorIndex,
        Func<UiaElementSnapshot, bool> predicate)
    {
        for (int index = ancestorIndex + 1; index < snapshot.Elements.Count; index++)
        {
            UiaElementSnapshot element = snapshot.Elements[index];
            if (IsDescendantOf(snapshot, index, ancestorIndex) && predicate(element))
            {
                return element;
            }
        }

        throw new InvalidOperationException($"No matching descendant was found for element {ancestorIndex}.");
    }

    private static bool IsDescendantOf(UiaSnapshot snapshot, int elementIndex, int ancestorIndex)
    {
        int parentIndex = snapshot.Elements[elementIndex].ParentIndex;
        while (parentIndex >= 0)
        {
            if (parentIndex == ancestorIndex)
            {
                return true;
            }

            parentIndex = snapshot.Elements[parentIndex].ParentIndex;
        }

        return false;
    }

    private static bool HasDistinctNativeAncestor(UiaSnapshot snapshot, int elementIndex, int rootWindowHandle)
    {
        int parentIndex = snapshot.Elements[elementIndex].ParentIndex;
        while (parentIndex >= 0)
        {
            int nativeWindowHandle = snapshot.Elements[parentIndex].NativeWindowHandle;
            if (nativeWindowHandle != 0 && nativeWindowHandle != rootWindowHandle)
            {
                return true;
            }

            parentIndex = snapshot.Elements[parentIndex].ParentIndex;
        }

        return false;
    }
}