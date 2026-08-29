// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>WM_PRINTCLIENT</c> <c>wParam</c> as a target device-context handle.
    /// </summary>
    /// <param name="wParam">The message <c>wParam</c> that carries the destination <c>HDC</c>.</param>
    public readonly ref struct PrintClient(WPARAM wParam)
    {
        /// <summary>
        ///  Gets the raw destination device-context handle from <c>wParam</c>.
        /// </summary>
        public HDC HDC { get; } = new((nint)wParam.Value);

        /// <summary>
        ///  Gets a managed wrapper around <see cref="HDC"/>.
        /// </summary>
        public DeviceContext DeviceContext => DeviceContext.Create(HDC);
    }
}