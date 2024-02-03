// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

[Flags]
public enum ClippingPrecision : byte
{
    /// <summary>
    ///  Use default clipping.
    /// </summary>
    Default = FONT_CLIP_PRECISION.CLIP_DEFAULT_PRECIS,

    /// <summary>
    ///  Not used.
    /// </summary>
    Character = FONT_CLIP_PRECISION.CLIP_CHARACTER_PRECIS,

    /// <summary>
    ///  Returned when enumerating fonts.
    /// </summary>
    Stroke = FONT_CLIP_PRECISION.CLIP_STROKE_PRECIS,

    /// <summary>
    ///  Controls font rotation. Set to rotate according to the orientation of the coordinate system.
    ///  If not set device fonts rotate counterclockwise, otherwise follows coordinate system.
    /// </summary>
    Angles = FONT_CLIP_PRECISION.CLIP_LH_ANGLES,

    /// <summary>
    ///  Not used.
    /// </summary>
    TrueTypeAlways = FONT_CLIP_PRECISION.CLIP_TT_ALWAYS,

    /// <summary>
    ///  Disable font association.
    /// </summary>
    FontAssociationDisable = FONT_CLIP_PRECISION.CLIP_DFA_DISABLE,

    /// <summary>
    ///  Use font embedding to render document content.
    /// </summary>
    Embedded = FONT_CLIP_PRECISION.CLIP_EMBEDDED
}