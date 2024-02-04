// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Pen pixel mix modes (ROP2)
/// </summary>
public enum PenMixMode : int
{
    // https://learn.microsoft.com/openspecs/windows_protocols/ms-mnpr/1f681fd4-8379-4312-8bf9-138e69f4f12b
    // https://learn.microsoft.com/windows/win32/gdi/binary-raster-operations?redirectedfrom=MSDN

    /// <summary>
    ///  Pixel is always drawn black. [R2_BLACK]
    /// </summary>
    Black = R2_MODE.R2_BLACK,

    /// <summary>
    ///  Inverse of result of MergePen. [R2_NOTMERGEPEN]
    /// </summary>
    NotMergePen = R2_MODE.R2_NOTMERGEPEN,

    /// <summary>
    ///  The pixel is a combination of the colors that are common to both the screen and the inverse of the pen. [R2_MASKNOTPEN]
    /// </summary>
    MaskNotPen = R2_MODE.R2_MASKNOTPEN,

    /// <summary>
    ///  The pixel is the inverse of the pen color. [R2_NOTCOPYPEN]
    /// </summary>
    NotCopyPen = R2_MODE.R2_NOTCOPYPEN,

    /// <summary>
    ///  The pixel is a combination of the colors that are common to both the pen and the inverse of the screen. [R2_MASKPENNOT]
    /// </summary>
    MaskPenNot = R2_MODE.R2_MASKPENNOT,

    /// <summary>
    ///  The pixel is the inverse of the screen color. [R2_NOT]
    /// </summary>
    Not = R2_MODE.R2_NOT,

    /// <summary>
    ///  The pixel is a combination of the colors in the pen and in the screen, but not in both. [R2_XORPEN]
    /// </summary>
    XOrPen = R2_MODE.R2_XORPEN,

    /// <summary>
    ///  Inverse of the result of MaskPen. [R2_NOTMASKPEN]
    /// </summary>
    NotMaskPen = R2_MODE.R2_NOTMASKPEN,

    /// <summary>
    ///  The pixel is a combination of the colors that are common to both the pen and the screen. [R2_MASKPEN]
    /// </summary>
    MaskPen = R2_MODE.R2_MASKPEN,

    /// <summary>
    ///  Inverse of the result of XOrPen. [R2_NOTXORPEN]
    /// </summary>
    NotXOrPen = R2_MODE.R2_NOTXORPEN,

    /// <summary>
    ///  The pixel remains unchanged. [R2_NOP]
    /// </summary>
    Nop = R2_MODE.R2_NOP,

    /// <summary>
    ///  The pixel is a combination of the screen color and the inverse of the pen color. [R2_MERGENOTPEN]
    /// </summary>
    MergeNotPen = R2_MODE.R2_MERGENOTPEN,

    /// <summary>
    ///  The pixel always has the color of the pen. [R2_COPYPEN]
    /// </summary>
    CopyPen = R2_MODE.R2_COPYPEN,

    /// <summary>
    ///  The pixel is a combination of the pen color and the inverse of the screen color. [R2_MERGEPENNOT]
    /// </summary>
    MergePenNot = R2_MODE.R2_MERGEPENNOT,

    /// <summary>
    ///  The pixel is a combination of the pen color and the screen color. [R2_MERGEPEN]
    /// </summary>
    MergePen = R2_MODE.R2_MERGEPEN,

    /// <summary>
    ///  The pixel is always drawn as white. [R2_WHITE]
    /// </summary>
    White = R2_MODE.R2_WHITE
}