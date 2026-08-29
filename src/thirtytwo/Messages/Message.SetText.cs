// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>WM_SETTEXT</c> <c>lParam</c> as a null-terminated UTF-16 string pointer.
    /// </summary>
    /// <param name="lParam">The message <c>lParam</c> string pointer.</param>
    public unsafe readonly ref struct SetText(LPARAM lParam)
    {
        /// <summary>
        ///  Gets the requested window/control text as a span.
        /// </summary>
        public ReadOnlySpan<char> Text { get; } = Conversion.NullTerminatedStringToSpan((char*)lParam);
    }
}