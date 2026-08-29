// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.ViewManagement;

/// <summary>
///  Represents the ABI layout of <c>Windows.UI.Color</c>.
/// </summary>
internal struct UISettingsColor
{
    /// <summary>
    ///  Alpha channel value.
    /// </summary>
    internal byte A;

    /// <summary>
    ///  Red channel value.
    /// </summary>
    internal byte R;

    /// <summary>
    ///  Green channel value.
    /// </summary>
    internal byte G;

    /// <summary>
    ///  Blue channel value.
    /// </summary>
    internal byte B;
}