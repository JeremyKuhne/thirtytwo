// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.WinUI;

/// <summary>Provides a pending text selection change.</summary>
public sealed class WinUITextSelectionChangingEventArgs(int selectionStart, int selectionLength) : CancelEventArgs
{
    /// <summary>Gets the proposed selection start.</summary>
    public int SelectionStart { get; } = selectionStart;

    /// <summary>Gets the proposed selection length.</summary>
    public int SelectionLength { get; } = selectionLength;
}