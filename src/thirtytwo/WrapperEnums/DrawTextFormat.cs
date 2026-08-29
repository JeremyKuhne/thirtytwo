// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Specifies formatting options for drawing text.
/// </summary>
[Flags]
public enum DrawTextFormat : uint
{
    /// <summary>
    ///  Justifies the text to the top of the rectangle.
    /// </summary>
    Top = DRAW_TEXT_FORMAT.DT_TOP,

    /// <summary>
    ///  Aligns text to the left of the rectangle.
    /// </summary>
    Left = DRAW_TEXT_FORMAT.DT_LEFT,

    /// <summary>
    ///  Centers text horizontally in the rectangle.
    /// </summary>
    Center = DRAW_TEXT_FORMAT.DT_CENTER,

    /// <summary>
    ///  Aligns text to the right of the rectangle.
    /// </summary>
    Right = DRAW_TEXT_FORMAT.DT_RIGHT,

    /// <summary>
    ///  Centers text vertically.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Only works with <see cref="SingleLine"/>.
    ///  </para>
    /// </remarks>
    VerticallyCenter = DRAW_TEXT_FORMAT.DT_VCENTER,

    /// <summary>
    ///  Justifies the text to the bottom of the rectangle.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Only works with <see cref="SingleLine"/>.
    ///  </para>
    /// </remarks>
    Bottom = DRAW_TEXT_FORMAT.DT_BOTTOM,

    /// <summary>
    ///  Breaks lines between words when a word would extend past the edge of the rectangle.
    /// </summary>
    WordBreak = DRAW_TEXT_FORMAT.DT_WORDBREAK,

    /// <summary>
    ///  Displays text on one line; carriage returns and line feeds do not break the line.
    /// </summary>
    SingleLine = DRAW_TEXT_FORMAT.DT_SINGLELINE,

    /// <summary>
    ///  Expands tab characters, using eight characters per tab by default.
    /// </summary>
    ExpandTabs = DRAW_TEXT_FORMAT.DT_EXPANDTABS,

    /// <summary>
    ///  Uses bits 8 through 15 of the format value to specify the number of characters per tab stop.
    /// </summary>
    TabStop = DRAW_TEXT_FORMAT.DT_TABSTOP,

    /// <summary>
    ///  Draws without clipping text to the specified rectangle.
    /// </summary>
    NoClip = DRAW_TEXT_FORMAT.DT_NOCLIP,

    /// <summary>
    ///  Includes the font's external leading in the height of each line.
    /// </summary>
    ExternalLeading = DRAW_TEXT_FORMAT.DT_EXTERNALLEADING,

    /// <summary>
    ///  Calculates the rectangle required by the formatted text without drawing the text.
    /// </summary>
    CalculateRectangle = DRAW_TEXT_FORMAT.DT_CALCRECT,

    /// <summary>
    ///  Disables processing of ampersand mnemonic-prefix characters.
    /// </summary>
    NoPrefix = DRAW_TEXT_FORMAT.DT_NOPREFIX,

    /// <summary>
    ///  Uses the system font to calculate text metrics.
    /// </summary>
    Internal = DRAW_TEXT_FORMAT.DT_INTERNAL,

    /// <summary>
    ///  Uses the text-display behavior of a multiline edit control and omits a partially visible final line.
    /// </summary>
    EditControl = DRAW_TEXT_FORMAT.DT_EDITCONTROL,

    /// <summary>
    ///  Replaces characters in the middle with an ellipsis, preserving as much text as possible after the last backslash.
    /// </summary>
    PathEllipsis = DRAW_TEXT_FORMAT.DT_PATH_ELLIPSIS,

    /// <summary>
    ///  Truncates text that does not fit at the end of the rectangle and adds an ellipsis.
    /// </summary>
    EndEllipsis = DRAW_TEXT_FORMAT.DT_END_ELLIPSIS,

    /// <summary>
    ///  Modifies the supplied string to match displayed text when <see cref="EndEllipsis"/> or
    ///  <see cref="PathEllipsis"/> is specified.
    /// </summary>
    ModifyString = DRAW_TEXT_FORMAT.DT_MODIFYSTRING,

    /// <summary>
    ///  Uses right-to-left reading order for bidirectional text when a Hebrew or Arabic font is selected.
    /// </summary>
    RightToLeftReading = DRAW_TEXT_FORMAT.DT_RTLREADING,

    /// <summary>
    ///  Truncates any word that does not fit in the rectangle and adds an ellipsis.
    /// </summary>
    WordEllipsis = DRAW_TEXT_FORMAT.DT_WORD_ELLIPSIS,

    /// <summary>
    ///  Prevents line breaks at double-wide characters when <see cref="WordBreak"/> is specified.
    /// </summary>
    NoFullWidthCharacterBreak = DRAW_TEXT_FORMAT.DT_NOFULLWIDTHCHARBREAK,

    /// <summary>
    ///  Processes ampersand mnemonic prefixes without underlining the following character.
    /// </summary>
    HidePrefix = DRAW_TEXT_FORMAT.DT_HIDEPREFIX,

    /// <summary>
    ///  Draws only the underline at the position identified by an ampersand mnemonic prefix.
    /// </summary>
    PrefixOnly = DRAW_TEXT_FORMAT.DT_PREFIXONLY
}