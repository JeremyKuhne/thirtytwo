// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Represents an immutable snapshot of the application's resolved color state.</summary>
public sealed record ApplicationColorState
{
    internal ApplicationColorState(
        ApplicationColorMode requestedMode,
        bool isDark,
        bool isHighContrast,
        bool useUndocumentedDarkModeApis,
        bool undocumentedDarkModeApisSupported,
        int generation,
        ApplicationColorPalette palette)
    {
        RequestedMode = requestedMode;
        IsDark = isDark;
        IsHighContrast = isHighContrast;
        UseUndocumentedDarkModeApis = useUndocumentedDarkModeApis;
        UndocumentedDarkModeApisSupported = undocumentedDarkModeApisSupported;
        Generation = generation;
        Palette = palette;
    }

    /// <summary>Gets the mode requested through <see cref="Application.ColorMode"/>.</summary>
    public ApplicationColorMode RequestedMode { get; }

    /// <summary>Gets whether the resolved Windows application preference is Dark.</summary>
    /// <remarks>
    ///  <para>
    ///   When <see cref="IsHighContrast"/> is <see langword="true"/>, the palette contains system High Contrast
    ///   colors regardless of this value.
    ///  </para>
    /// </remarks>
    public bool IsDark { get; }

    /// <summary>Gets whether Windows High Contrast is active.</summary>
    public bool IsHighContrast { get; }

    /// <summary>Gets whether private Windows dark-mode APIs are enabled for native control compatibility.</summary>
    public bool UseUndocumentedDarkModeApis { get; }

    /// <summary>Gets whether the required private Windows dark-mode exports are available.</summary>
    /// <remarks>
    ///  <para>
    ///   A native wrapper can use this with <see cref="UseUndocumentedDarkModeApis"/> to decide whether it needs a
    ///   documented rendering fallback. Availability does not indicate that a private theme was applied successfully
    ///   to any particular HWND.
    ///  </para>
    /// </remarks>
    public bool UndocumentedDarkModeApisSupported { get; }

    /// <summary>Gets the generation token for this application color state.</summary>
    /// <remarks>
    ///  <para>
    ///   Controls can compare this value for equality to invalidate resources cached from another snapshot. The
    ///   numeric value can wrap and does not provide an ordering contract.
    ///  </para>
    /// </remarks>
    public int Generation { get; }

    /// <summary>Gets the resolved semantic colors for native window and control rendering.</summary>
    public ApplicationColorPalette Palette { get; }
}