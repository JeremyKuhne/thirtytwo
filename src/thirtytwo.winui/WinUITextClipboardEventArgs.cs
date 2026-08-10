// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Provides a clipboard operation that can be handled by the application.</summary>
public sealed class WinUITextClipboardEventArgs : EventArgs
{
    /// <summary>Gets or sets whether the clipboard operation was handled.</summary>
    public bool Handled { get; set; }
}