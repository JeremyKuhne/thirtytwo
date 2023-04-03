// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HBITMAP : IHandle<HBITMAP>
{
    HBITMAP IHandle<HBITMAP>.Handle => this;
    object? IHandle<HBITMAP>.Wrapper => null;

    public static implicit operator HGDIOBJ(HBITMAP handle) => new(handle.Value);
}
