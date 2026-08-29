// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>
///  Provides an IME text-composition range.
/// </summary>
/// <param name="startIndex">The zero-based start index of the composition span.</param>
/// <param name="length">The length of the composition span in characters.</param>
public sealed class WinUITextCompositionEventArgs(int startIndex, int length) : EventArgs
{
    /// <summary>
    ///  Gets the zero-based start index of the composition span.
    /// </summary>
    public int StartIndex { get; } = startIndex;

    /// <summary>
    ///  Gets the composition span length in characters.
    /// </summary>
    public int Length { get; } = length;
}