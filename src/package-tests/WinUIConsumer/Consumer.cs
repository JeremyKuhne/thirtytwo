// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WinUIConsumer;

public static class Consumer
{
    public static Type EnvironmentType => typeof(Windows.WinUI.XamlHostEnvironment);

    public static Type HostControlType => typeof(Windows.WinUI.XamlHostControl);

    public static Type HostContextType => typeof(Windows.WinUI.XamlHostContext);

    public static Type ColorPickerType => typeof(Windows.WinUI.WinUIColorPicker);

    public static Type TextBoxType => typeof(Windows.WinUI.WinUITextBox);

    public static Type RichEditBoxType => typeof(Windows.WinUI.WinUIRichEditBox);

    public static void ConfigureColorPicker(Windows.WinUI.WinUIColorPicker colorPicker)
    {
        colorPicker.IsAlphaEnabled = true;
        colorPicker.IsColorSpectrumVisible = true;
        colorPicker.IsColorPreviewVisible = true;
        colorPicker.IsColorSliderVisible = true;
        colorPicker.IsColorChannelTextInputVisible = true;
        colorPicker.IsAlphaSliderVisible = true;
        colorPicker.IsAlphaTextInputVisible = true;
        colorPicker.IsHexInputVisible = true;
        colorPicker.ColorSpectrumShape = Windows.WinUI.WinUIColorSpectrumShape.Box;
        colorPicker.ColorSpectrumComponents = Windows.WinUI.WinUIColorSpectrumComponents.HueSaturation;
        colorPicker.Orientation = Windows.WinUI.WinUIColorPickerOrientation.Vertical;
        colorPicker.RequestedTheme = Windows.WinUI.WinUIElementTheme.Light;
    }

    public static Windows.WinUI.XamlHostControl ConfigureTextBox(Windows.WinUI.WinUITextBox textBox)
    {
        textBox.AcceptsReturn = true;
        textBox.PlaceholderText = "Plain text";
        textBox.Text = "TextBox";
        textBox.SelectAll();
        return textBox;
    }

    public static Windows.WinUI.XamlHostControl ConfigureRichEditBox(Windows.WinUI.WinUIRichEditBox richEditBox)
    {
        richEditBox.AcceptsReturn = true;
        richEditBox.ClipboardCopyFormat = Windows.WinUI.WinUIRichEditClipboardFormat.PlainText;
        richEditBox.DisabledFormattingAccelerators = Windows.WinUI.WinUIRichEditDisabledFormattingAccelerators.Bold;
        richEditBox.Text = "RichEditBox";
        _ = richEditBox.Document;
        _ = richEditBox.TextDocument;
        return richEditBox;
    }
}