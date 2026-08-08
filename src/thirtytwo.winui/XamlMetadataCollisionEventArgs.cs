// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>
///  Reports that multiple metadata providers resolved the same XAML type.
/// </summary>
public sealed class XamlMetadataCollisionEventArgs(
    string requestedType,
    Type winningProviderType,
    Type conflictingProviderType) : EventArgs
{
    /// <summary>Gets the requested XAML type name.</summary>
    public string RequestedType { get; } = requestedType;

    /// <summary>Gets the first provider, whose result wins.</summary>
    public Type WinningProviderType { get; } = winningProviderType;

    /// <summary>Gets the later provider that also resolved the type.</summary>
    public Type ConflictingProviderType { get; } = conflictingProviderType;
}