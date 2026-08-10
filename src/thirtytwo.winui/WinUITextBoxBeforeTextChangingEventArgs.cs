// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.WinUI;

/// <summary>Provides text before a TextBox change is committed.</summary>
public sealed class WinUITextBoxBeforeTextChangingEventArgs(string newText) : CancelEventArgs
{
    /// <summary>Gets the proposed text.</summary>
    public string NewText { get; } = newText;
}