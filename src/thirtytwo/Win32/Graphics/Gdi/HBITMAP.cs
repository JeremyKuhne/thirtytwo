// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

public partial struct HBITMAP : IHandle<HBITMAP>
{
    HBITMAP IHandle<HBITMAP>.Handle => this;
    object? IHandle<HBITMAP>.Wrapper => null;
}