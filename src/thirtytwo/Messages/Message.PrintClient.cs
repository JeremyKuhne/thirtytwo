// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref struct PrintClient(WPARAM wParam)
    {
        public HDC HDC { get; } = new((nint)wParam.Value);

        public DeviceContext DeviceContext => DeviceContext.Create(HDC);
    }
}