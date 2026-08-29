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

    /// <summary>
    ///  Initializes a feature with an explicit parameter value.
    /// </summary>
    /// <param name="nameTag">The OpenType feature tag.</param>
    /// <param name="parameter">The feature parameter value used by DirectWrite.</param>
    public FontFeature(FontFeatureTag nameTag, uint parameter)
    {
        NameTag = nameTag;
        Parameter = parameter;
    }

    /// <summary>
    ///  Initializes a feature with enable or disable semantics.
    /// </summary>
    /// <param name="nameTag">The OpenType feature tag.</param>
    /// <param name="enable">
    ///  <see langword="true"/> to pass parameter value <c>1</c>; otherwise parameter value <c>0</c>.
    /// </param>
    public FontFeature(FontFeatureTag nameTag, bool enable = true)
    {
        NameTag = nameTag;
        Parameter = enable ? 1u : 0u;
    }

    /// <summary>
    ///  Creates an enabled feature from a tag.
    /// </summary>
    /// <param name="tag">The OpenType feature tag.</param>
    /// <returns>A <see cref="FontFeature"/> whose <see cref="Parameter"/> is set to <c>1</c>.</returns>
    public static implicit operator FontFeature(FontFeatureTag tag) => new(tag);
}