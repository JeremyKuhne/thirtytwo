// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Page-space to device-space transformation.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/windows/win32/gdi/mapping-modes-and-translations">
///    Mapping Modes and Translations
///   </see>
///  </para>
/// </remarks>
public enum MappingMode
{
    /// <summary>
    ///  Pixel mapping. Each unit in page space is one pixel in device space.
    ///  X increases left to right. Y increases from top to bottom. (MM_TEXT)
    /// </summary>
    Text = HDC_MAP_MODE.MM_TEXT,

    /// <summary>
    ///  Each unit in page space is 0.1 mm in device space.
    ///  X increases left to right. Y increases from bottom to top. (MM_LOMETRIC)
    /// </summary>
    LowMetric = HDC_MAP_MODE.MM_LOMETRIC,

    /// <summary>
    ///  Each unit in page space is 0.01 mm in device space.
    ///  X increases left to right. Y increases from bottom to top. (MM_HIMETRIC)
    /// </summary>
    HighMetric = HDC_MAP_MODE.MM_HIMETRIC,

    /// <summary>
    ///  Each unit in page space is 0.01 inch in device space.
    ///  X increases left to right. Y increases from bottom to top. (MM_LOENGLISH)
    /// </summary>
    LowEnglish = HDC_MAP_MODE.MM_LOENGLISH,

    /// <summary>
    ///  Each unit in page space is 0.001 inch in device space.
    ///  X increases left to right. Y increases from bottom to top. (MM_HIENGLISH)
    /// </summary>
    HighEnglish = HDC_MAP_MODE.MM_HIENGLISH,

    /// <summary>
    ///  Each unit in page space is a twip in device space (1/1440 inch).
    ///  X increases left to right. Y increases from bottom to top. (MM_TWIPS)
    /// </summary>
    Twips = HDC_MAP_MODE.MM_TWIPS,

    /// <summary>
    ///  Application defined via SetWindow/ViewPortExtents.
    ///  Both axis are equally scaled. (MM_ISOTROPIC)
    /// </summary>
    Isotropic = HDC_MAP_MODE.MM_ISOTROPIC,

    /// <summary>
    ///  Application defined via SetWindow/ViewPortExtents.
    ///  Axis may not be equally scaled. (MM_ANISOTROPIC)
    /// </summary>
    Anisotropic = HDC_MAP_MODE.MM_ANISOTROPIC
}