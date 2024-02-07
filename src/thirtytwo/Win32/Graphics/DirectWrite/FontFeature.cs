// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <inheritdoc cref="DWRITE_FONT_FEATURE"/>
public readonly struct FontFeature
{
    /// <summary>
    ///  The feature OpenType name identifier.
    /// </summary>
    public readonly FontFeatureTag NameTag;

    /// <summary>
    ///  Execution parameter of the feature.
    /// </summary>
    /// <remarks>
    ///  The parameter should be non-zero to enable the feature. Once enabled, a feature can't be disabled again within
    ///  the same range. Features requiring a selector use this value to indicate the selector index.
    /// </remarks>
    public readonly uint Parameter;

    public FontFeature(FontFeatureTag nameTag, uint parameter)
    {
        NameTag = nameTag;
        Parameter = parameter;
    }

    public FontFeature(FontFeatureTag nameTag, bool enable = true)
    {
        NameTag = nameTag;
        Parameter = enable ? 1u : 0u;
    }

    public static implicit operator FontFeature(FontFeatureTag tag) => new(tag);
}