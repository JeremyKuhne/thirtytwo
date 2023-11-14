// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public static partial class Message
{
    public unsafe readonly ref struct SetText(LPARAM lParam)
    {
        public ReadOnlySpan<char> Text { get; } = Conversion.NullTerminatedStringToSpan((char*)lParam);
    }
}