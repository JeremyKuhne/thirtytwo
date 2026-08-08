// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

namespace Windows.WinUI;

/// <summary>
///  Reports that a later resource dictionary overrides an earlier key.
/// </summary>
public sealed class XamlResourceCollisionEventArgs(
    object key,
    ResourceDictionary overriddenDictionary,
    ResourceDictionary winningDictionary) : EventArgs
{
    /// <summary>Gets the duplicate resource key.</summary>
    public object Key { get; } = key;

    /// <summary>Gets the earlier dictionary whose value is overridden.</summary>
    public ResourceDictionary OverriddenDictionary { get; } = overriddenDictionary;

    /// <summary>Gets the later dictionary whose value wins.</summary>
    public ResourceDictionary WinningDictionary { get; } = winningDictionary;
}