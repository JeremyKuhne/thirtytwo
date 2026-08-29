// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Provides data for an accept-or-cancel decision in an event callback.
/// </summary>
public class AcceptEventArgs : EventArgs
{
    /// <summary>
    ///  Gets or sets whether the operation should proceed.
    /// </summary>
    public bool Accept { get; set; } = true;
}