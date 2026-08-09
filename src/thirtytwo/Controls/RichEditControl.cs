// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;
using Windows.Win32.UI.Controls.RichEdit;

namespace Windows;

/// <summary>
///  <see href="https://learn.microsoft.com/windows/win32/controls/about-rich-edit-controls#rich-edit-version-41">RichEdit 4.1</see> control wrapper.
/// </summary>
public partial class RichEditControl : EditBase
{
    private static readonly WindowClass s_richEditClass;

    static RichEditControl()
    {
        // Ensure RichEdit 4.1 is loaded
        if (PInvoke.LoadLibrary("Msftedit.dll").IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        s_richEditClass = new("RICHEDIT50W");
    }

    public RichEditControl(
        Rectangle bounds,
        string? text = default,
        Styles editStyle = Styles.Left,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            s_richEditClass,
            style |= (WindowStyles)editStyle,
            text,
            extendedStyle,
            parentWindow,
            parameters)
    {
        ApplyApplicationColors();
    }

    /// <inheritdoc/>
    protected override void OnColorModeChanged()
    {
        ApplyApplicationColors();
        base.OnColorModeChanged();
    }

    private unsafe void ApplyApplicationColors()
    {
        ApplicationColorState state = Application.CurrentColorState;
        ApplyApplicationDarkModeTheme(
            darkSubAppName: null,
            darkSubIdList: "DarkMode_Explorer::ScrollBar");

        Color background = state.Palette.ControlBackground;
        this.SendMessage(
            (MessageType)PInvoke.EM_SETBKGNDCOLOR,
            (WPARAM)(BOOL)state.IsHighContrast,
            (LPARAM)(nint)((COLORREF)background).Value);

        CHARFORMAT2W characterFormat = new();
        characterFormat.Base.cbSize = (uint)sizeof(CHARFORMAT2W);
        characterFormat.Base.dwMask = CFM_MASK.CFM_COLOR;
        characterFormat.Base.dwEffects = state.IsHighContrast ? CFE_EFFECTS.CFE_AUTOCOLOR : default;
        characterFormat.Base.crTextColor = (COLORREF)state.Palette.ControlForeground;

        this.SendMessage(
            (MessageType)PInvoke.EM_SETCHARFORMAT,
            default,
            (LPARAM)(nint)(&characterFormat));
        this.SendMessage(
            (MessageType)PInvoke.EM_SETCHARFORMAT,
            (WPARAM)PInvoke.SCF_ALL,
            (LPARAM)(nint)(&characterFormat));
    }
}