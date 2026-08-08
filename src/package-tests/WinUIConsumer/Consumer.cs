// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WinUIConsumer;

public static class Consumer
{
    public static Type EnvironmentType => typeof(Windows.WinUI.XamlHostEnvironment);

    public static Type HostControlType => typeof(Windows.WinUI.XamlHostControl);

    public static Type HostContextType => typeof(Windows.WinUI.XamlHostContext);

    public static Type ColorPickerType => typeof(Windows.WinUI.WinUIColorPicker);

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
}