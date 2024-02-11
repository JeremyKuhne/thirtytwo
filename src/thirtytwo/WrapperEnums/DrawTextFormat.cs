// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

[Flags]
public enum DrawTextFormat : uint
{
    /// <summary>
    ///  Justifies the text to the top of the rectangle.
    /// </summary>
    Top = DRAW_TEXT_FORMAT.DT_TOP,
    Left = DRAW_TEXT_FORMAT.DT_LEFT,
    Center = DRAW_TEXT_FORMAT.DT_CENTER,
    Right = DRAW_TEXT_FORMAT.DT_RIGHT,

    /// <summary>
    ///  Centers text vertically.
    /// </summary>
    /// <remarks>
    ///  <para>Only works with <see cref="SingleLine"/>.</para>
    /// </remarks>
    VerticallyCenter = DRAW_TEXT_FORMAT.DT_VCENTER,

    /// <summary>
    ///  Justifies the text to the bottom of the rectangle.
    /// </summary>
    /// <remarks>
    ///  <para>Only works with <see cref="SingleLine"/>.</para>
    /// </remarks>
    Bottom = DRAW_TEXT_FORMAT.DT_BOTTOM,
    WordBreak = DRAW_TEXT_FORMAT.DT_WORDBREAK,
    SingleLine = DRAW_TEXT_FORMAT.DT_SINGLELINE,
    ExpandTabs = DRAW_TEXT_FORMAT.DT_EXPANDTABS,
    TabStop = DRAW_TEXT_FORMAT.DT_TABSTOP,
    NoClip = DRAW_TEXT_FORMAT.DT_NOCLIP,
    ExternalLeading = DRAW_TEXT_FORMAT.DT_EXTERNALLEADING,
    CalculateRectangle = DRAW_TEXT_FORMAT.DT_CALCRECT,
    NoPrefix = DRAW_TEXT_FORMAT.DT_NOPREFIX,
    Internal = DRAW_TEXT_FORMAT.DT_INTERNAL,
    EditControl = DRAW_TEXT_FORMAT.DT_EDITCONTROL,
    PathEllipsis = DRAW_TEXT_FORMAT.DT_PATH_ELLIPSIS,
    EndEllipsis = DRAW_TEXT_FORMAT.DT_END_ELLIPSIS,
    ModifyString = DRAW_TEXT_FORMAT.DT_RIGHT,
    RightToLeftReading = DRAW_TEXT_FORMAT.DT_RTLREADING,
    WordEllipsis = DRAW_TEXT_FORMAT.DT_RIGHT,
    NoFullWidthCharacterBreak = DRAW_TEXT_FORMAT.DT_NOFULLWIDTHCHARBREAK,
    HidePrefix = DRAW_TEXT_FORMAT.DT_HIDEPREFIX,
    PrefixOnly = DRAW_TEXT_FORMAT.DT_PREFIXONLY
}