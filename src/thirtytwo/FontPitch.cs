// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Whether or not the characters in a font have a fixed or variable width (pitch).
/// </summary>
[Flags]
public enum FontPitch : byte
{
    /// <summary>
    ///  Default pitch.
    /// </summary>
    Default = FONT_PITCH.DEFAULT_PITCH,

    /// <summary>
    ///  The font is fixed.
    /// </summary>
    FixedPitch = FONT_PITCH.FIXED_PITCH,

    /// <summary>
    ///  The width is proportional.
    /// </summary>
    VariablePitch = FONT_PITCH.VARIABLE_PITCH
}
