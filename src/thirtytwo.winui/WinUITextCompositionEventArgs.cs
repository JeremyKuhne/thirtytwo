// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Provides an IME text-composition range.</summary>
public sealed class WinUITextCompositionEventArgs(int startIndex, int length) : EventArgs
{
    /// <summary>Gets the composition start index.</summary>
    public int StartIndex { get; } = startIndex;

    /// <summary>Gets the composition length.</summary>
    public int Length { get; } = length;
}