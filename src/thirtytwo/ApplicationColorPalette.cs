// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.UI.ViewManagement;

namespace Windows;

/// <summary>Contains immutable opaque semantic colors used by native windows and controls.</summary>
public sealed record ApplicationColorPalette
{
    // ------------------------------
    // Palette sources and derivation
    // ------------------------------
    //
    // Normative source:
    // -----------------
    //
    // The non-contrast palette follows the immutable theme resources shipped by Microsoft.WindowsAppSDK.WinUI
    // 2.3.0. This repository references Microsoft.WindowsAppSDK 2.3.1, whose resolved dependency graph selects
    // that WinUI component version. The source values are in lib/native/Microsoft.UI/Themes/generic.xaml. The
    // Default dictionary is the Dark theme and the Light dictionary is the Light theme.
    //
    // Sources:
    // https://learn.microsoft.com/windows/apps/develop/platform/xaml/xaml-theme-resources
    // https://github.com/microsoft/microsoft-ui-xaml/blob/main/src/controls/dev/CommonStyles/Common_themeresources_any.xaml
    //
    // Compositing:
    // ------------
    //
    // WinUI stores many colors as translucent ARGB tokens. GDI and DWM consumers need opaque colors, so each token
    // is composited over the surface on which this framework uses it. For each channel:
    //
    //     round((source * alpha + destination * (255 - alpha)) / 255)
    //
    // Text and disabled text are composited over their matching window or control fill. Border is composited over
    // the window surface because its current consumer is the top-level DWM frame. A control border would instead
    // composite the same raw token over ControlBackground.
    //
    // Raw WinUI 2.3.0 tokens (ARGB):
    // ------------------------------
    //
    // Dark:
    //
    // - SolidBackgroundFillColorBase=#FF202020
    // - TextFillColorPrimary=#FFFFFFFF
    // - TextFillColorDisabled=#5DFFFFFF
    // - ControlFillColorDefault=#0FFFFFFF
    // - ControlStrokeColorDefault=#12FFFFFF
    // - TextOnAccentFillColorSelectedText=#FFFFFFFF
    //
    // Light:
    //
    // - SolidBackgroundFillColorBase=#FFF3F3F3
    // - TextFillColorPrimary=#E4000000
    // - TextFillColorDisabled=#5C000000
    // - ControlFillColorDefault=#B3FFFFFF
    // - ControlStrokeColorDefault=#0F000000
    // - TextOnAccentFillColorSelectedText=#FFFFFFFF
    //
    // Related WinUI tokens reserved for future state-specific drawing:
    //
    // These are recorded so custom drawing does not invent another ramp. Dark ControlFillColorSecondary=#15FFFFFF,
    // ControlFillColorTertiary=#08FFFFFF, and ControlFillColorDisabled=#0BFFFFFF composite over #202020 to #323232,
    // #272727, and #2A2A2A. Light ControlFillColorSecondary=#80F9F9F9, ControlFillColorTertiary=#4DF9F9F9, and
    // ControlFillColorDisabled=#4DF9F9F9 composite over #F3F3F3 to #F6F6F6, #F5F5F5, and #F5F5F5. WinUI maps
    // these to normal pointer-over, pressed, and disabled control backgrounds respectively.
    //
    // Dark focus tokens are FocusStrokeColorOuter=#FFFFFFFF and FocusStrokeColorInner=#B3000000; over #2D2D2D they
    // are #FFFFFF and #0D0D0D. Light focus tokens are FocusStrokeColorOuter=#E4000000 and
    // FocusStrokeColorInner=#B3FFFFFF; over #FBFBFB they are #1B1B1B and #FEFEFE. WinUI tooltip resources use
    // background #2B2B2B with foreground #FFFFFF in Dark, and background #F2F2F2 with effective foreground #1A1A1A
    // in Light. This type does not expose these roles until a native focus or tooltip adapter consumes them.
    //
    // Opaque values produced for this type:
    // -------------------------------------
    //
    // Dark:
    //
    // - WindowBackground=#202020; WindowForeground=#FFFFFF
    // - ControlBackground=#2D2D2D; ControlForeground=#FFFFFF
    // - DisabledForeground=#7A7A7A; Border=#303030; SelectionForeground=#FFFFFF
    // - The disabled token over the window surface would be #717171.
    // - The border token over the control surface would be #3C3C3C.
    //
    // Light:
    //
    // - WindowBackground=#F3F3F3; WindowForeground=#1A1A1A
    // - ControlBackground=#FBFBFB; ControlForeground=#1B1B1B
    // - DisabledForeground=#A0A0A0; Border=#E5E5E5; SelectionForeground=#FFFFFF
    // - The disabled token over the window surface would be #9B9B9B.
    // - The border token over the control surface would be #ECECEC.
    //
    // WCAG relative-luminance checks:
    //
    // - Dark window text: 16.29:1
    // - Dark control text: 13.77:1
    // - Dark disabled control text: 3.21:1
    // - Light window text: 15.68:1
    // - Light control text: 16.65:1
    // - Light disabled control text: 2.53:1
    //
    // Disabled text is exempt from the WCAG text contrast requirement but remains recorded so changes are deliberate.
    //
    // Dynamic accent:
    // ---------------
    //
    // WinUI defines AccentFillColorSelectedTextBackgroundBrush with SystemAccentColor. SelectionBackground therefore
    // comes from the documented Windows.UI.ViewManagement.UISettings.GetColorValue(UIColorType.Accent) WinRT API.
    // If activation or the call fails, the documented GetSysColor(COLOR_HIGHLIGHT) value is used. SelectionForeground
    // follows TextOnAccentFillColorSelectedText.
    //
    // Sources:
    // https://learn.microsoft.com/uwp/api/windows.ui.viewmanagement.uisettings.getcolorvalue
    // https://learn.microsoft.com/uwp/api/windows.ui.viewmanagement.uicolortype
    //
    // For emphasized controls, WinUI's AccentFillColorDefaultBrush uses SystemAccentColorLight2 in Dark and
    // SystemAccentColorDark1 in Light. Its pointer-over and pressed variants apply brush opacities 0.9 and 0.8.
    // Those accent-control roles are not currently represented by this type.
    //
    // High Contrast:
    // --------------
    // High Contrast does not use the WinUI Light or Dark ramp. Values come from the documented system color pairs
    // COLOR_WINDOW/COLOR_WINDOWTEXT, COLOR_3DFACE/COLOR_BTNTEXT, COLOR_GRAYTEXT, and
    // COLOR_HIGHLIGHT/COLOR_HIGHLIGHTTEXT. The frame uses COLOR_WINDOWTEXT, which Microsoft documents for app and
    // window borders on Windows 10 and 11.
    //
    // Source:
    // https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getsyscolor
    //
    // Research probes (observations, not palette contracts):
    // ------------------------------------------------------
    //
    // - On Windows build 10.0.26200.0, UISettings.GetColorValue returned Background=#FF000000,
    //   Foreground=#FFFFFFFF, AccentDark3=#FF001A68, AccentDark2=#FF003E92, AccentDark1=#FF0067C0,
    //   Accent=#FF0078D4, AccentLight1=#FF0091F8, AccentLight2=#FF4CC2FF, and AccentLight3=#FF99EBFF. White over
    //   the measured Accent had 4.53:1 contrast.
    //
    // - On the same build, UISettings.UIElementColor returned classic Light values rather than modern dark colors:
    //   ActiveCaption=#99B4D1, Background=#4A5459, ButtonFace=#F0F0F0, ButtonText=#000000, CaptionText=#000000,
    //   GrayText=#6D6D6D, Highlight=#0078D7, HighlightText=#FFFFFF, Hotlight=#0066CC,
    //   InactiveCaption=#BFCDDB, InactiveCaptionText=#000000, Window=#FFFFFF, and WindowText=#000000. AccentColor,
    //   TextHigh, TextMedium, TextLow, TextContrastWithHigh, NonTextHigh, NonTextMediumHigh, NonTextMedium,
    //   NonTextMediumLow, NonTextLow, PageBackground, PopupBackground, and OverlayOutsidePopup all returned
    //   transparent #00000000. It is therefore not a modern Light/Dark palette provider.
    //
    // - DwmGetColorizationColor returned #E3006FC4 with opaque blending enabled while UISettings Accent was
    //   #FF0078D4. DWM colorization is a chrome composition color, not the WinUI accent resource.
    //
    // - With the undocumented native dark-mode compatibility option enabled, captured common controls used dominant
    //   Button and ComboBox fill #333333, borders #9B9B9B/#8B8B8B, and checked glyph #60CDFF. On build 26200,
    //   DarkMode_Explorer drew the selected radio label #000000 even though WM_CTLCOLORBTN set the semantic control
    //   foreground; DarkMode_DarkTheme rendered it #FFFFFF. These are build- and state-specific pixels.
    //
    // - Documented GetThemeColor returned HRESULT 0x80070490 for most fill and border properties of those
    //   bitmap-backed dark parts. Push-button text reported #000000 for normal/hot/pressed/defaulted and #838383 for
    //   disabled. Checkbox/radio normal text was absent and disabled text was #6D6D6D. ComboBox borders reported
    //   #ABADB3 and disabled text #6D6D6D; these are inherited Light values. Documented DrawThemeBackground rendered
    //   a Light Button with dominant fill #FDFDFD and borders #D0D0D0/#BABABA, and a Light ComboBox with fill #FDFDFD
    //   and borders #D2D2D2/#BCBCBC, even for HWNDs visibly using the private dark association. Explicit
    //   DarkMode_Explorer::Button failed to open; CFD::ComboBox still rendered Light. Rendered sampling is retained
    //   only as compatibility-test evidence, not as a production color provider.
    //
    // - WinUI's XamlControlsResources exposes runtime resource dictionaries but requires Windows App SDK and XAML
    //   initialization, so it is suitable for the optional WinUI package rather than the Windows-App-SDK-independent
    //   core. ColorPaletteResources is an override dictionary, not a system palette reader.
    //   Microsoft.UI.System.ThemeSettings reports High Contrast state and scheme only; it exposes no colors.

    internal ApplicationColorPalette(
        Color windowBackground,
        Color windowForeground,
        Color controlBackground,
        Color controlForeground,
        Color disabledForeground,
        Color border,
        Color selectionBackground,
        Color selectionForeground)
    {
        WindowBackground = windowBackground;
        WindowForeground = windowForeground;
        ControlBackground = controlBackground;
        ControlForeground = controlForeground;
        DisabledForeground = disabledForeground;
        Border = border;
        SelectionBackground = selectionBackground;
        SelectionForeground = selectionForeground;
    }

    /// <summary>Gets the default top-level window and inherited custom-control background.</summary>
    public Color WindowBackground { get; }

    /// <summary>Gets the default foreground for text drawn directly on <see cref="WindowBackground"/>.</summary>
    public Color WindowForeground { get; }

    /// <summary>Gets the default background for interactive native control surfaces.</summary>
    public Color ControlBackground { get; }

    /// <summary>Gets the default foreground for text drawn on <see cref="ControlBackground"/>.</summary>
    public Color ControlForeground { get; }

    /// <summary>Gets the default disabled foreground for text drawn on <see cref="ControlBackground"/>.</summary>
    public Color DisabledForeground { get; }

    /// <summary>Gets the default top-level window border color.</summary>
    public Color Border { get; }

    /// <summary>Gets the default selected-text background.</summary>
    public Color SelectionBackground { get; }

    /// <summary>Gets the default selected-text foreground.</summary>
    public Color SelectionForeground { get; }

    internal static ApplicationColorPalette Create(bool dark, bool highContrast)
    {
        if (highContrast)
        {
            return new(
                GetSystemColor(SystemColor.Window),
                GetSystemColor(SystemColor.WindowText),
                GetSystemColor(SystemColor.ButtonFace),
                GetSystemColor(SystemColor.ButtonText),
                GetSystemColor(SystemColor.GrayText),
                GetSystemColor(SystemColor.WindowText),
                GetSystemColor(SystemColor.Highlight),
                GetSystemColor(SystemColor.HightlightText));
        }

        Color windowBackground = dark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(243, 243, 243);
        Color textFillPrimary = dark ? Color.White : Color.FromArgb(228, 0, 0, 0);
        Color textFillDisabled = dark ? Color.FromArgb(93, 255, 255, 255) : Color.FromArgb(92, 0, 0, 0);
        Color controlFillDefault = dark ? Color.FromArgb(15, 255, 255, 255) : Color.FromArgb(179, 255, 255, 255);
        Color controlStrokeDefault = dark ? Color.FromArgb(18, 255, 255, 255) : Color.FromArgb(15, 0, 0, 0);
        Color controlBackground = Composite(controlFillDefault, windowBackground);

        return new(
            windowBackground,
            Composite(textFillPrimary, windowBackground),
            controlBackground,
            Composite(textFillPrimary, controlBackground),
            Composite(textFillDisabled, controlBackground),
            Composite(controlStrokeDefault, windowBackground),
            GetSelectionBackground(),
            Color.White);
    }

    private static Color Composite(Color source, Color destination)
    {
        int inverseAlpha = byte.MaxValue - source.A;
        return Color.FromArgb(
            CompositeChannel(source.R, source.A, destination.R, inverseAlpha),
            CompositeChannel(source.G, source.A, destination.G, inverseAlpha),
            CompositeChannel(source.B, source.A, destination.B, inverseAlpha));
    }

    private static int CompositeChannel(int source, int alpha, int destination, int inverseAlpha)
        => ((source * alpha) + (destination * inverseAlpha) + 127) / byte.MaxValue;

    private static Color GetSelectionBackground()
        => SystemColorModeProvider.TryGetColor(UISettingsColorType.Accent, out UISettingsColor accent)
            ? Color.FromArgb(accent.A, accent.R, accent.G, accent.B)
            : GetSystemColor(SystemColor.Highlight);

    private static Color GetSystemColor(SystemColor color)
        => new COLORREF(PInvoke.GetSysColor((SYS_COLOR_INDEX)color));
}