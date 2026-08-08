// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace CoreOnlyConsumer;

public sealed class ThemedControl : Windows.CustomControl
{
    public ThemedControl()
    {
        ApplyApplicationDarkModeTheme("DarkMode_Explorer");
    }

    public Windows.ApplicationColorPalette Palette => Windows.Application.CurrentColorState.Palette;

    public Color EffectiveWindowBackground => GetEffectiveBackgroundColor();

    public Color EffectiveControlBackground => GetEffectiveBackgroundColor(controlSurface: true);

    public Color EffectiveWindowForeground => GetEffectiveForegroundColor(controlSurface: false);

    public Color EffectiveControlForeground => GetEffectiveForegroundColor();

    public void ApplyNativeTheme(string darkThemeName)
    {
        ApplyApplicationDarkModeTheme(darkThemeName);
        ApplyApplicationDarkModeTheme(Handle, darkThemeName);
    }

    protected override void OnColorModeChanged()
    {
        _ = Windows.Application.CurrentColorState;
        ApplyApplicationDarkModeTheme("DarkMode_Explorer");
        base.OnColorModeChanged();
    }
}