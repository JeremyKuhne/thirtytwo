// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>
///  Provides information while editor content is changing.
/// </summary>
/// <param name="isContentChanging">
///  The WinUI content-changing flag indicating whether the text content itself is being modified.
/// </param>
public sealed class WinUITextChangingEventArgs(bool isContentChanging) : EventArgs
{
    /// <summary>
    ///  Gets whether editor content is changing.
    /// </summary>
    public bool IsContentChanging { get; } = isContentChanging;
}