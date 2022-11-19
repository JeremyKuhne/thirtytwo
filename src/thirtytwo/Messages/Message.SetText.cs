// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public static partial class Message
{
    public readonly ref struct SetText
    {
        public ReadOnlySpan<char> Text { get; }

        public unsafe SetText(LPARAM lParam)
        {
            Text = Conversion.NullTerminatedStringToSpan((char*)lParam);
        }
    }
}