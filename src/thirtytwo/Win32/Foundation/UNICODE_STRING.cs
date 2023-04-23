// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe partial struct UNICODE_STRING
{
    public readonly int LengthInChars => Length / sizeof(char);
    public readonly int MaximumLengthInChars => Length / sizeof(char);

    [UnscopedRef]
    public readonly ReadOnlySpan<char> CurrentValue => new(Buffer, LengthInChars);

    [UnscopedRef]
    public readonly Span<char> FullBuffer => new(Buffer, MaximumLengthInChars);

    public override readonly string ToString() => CurrentValue.ToString();
}