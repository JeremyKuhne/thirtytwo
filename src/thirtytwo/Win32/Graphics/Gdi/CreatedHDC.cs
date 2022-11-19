// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Gdi;

public partial struct CreatedHDC : IDisposable, IHandle<HDC>, IHandle<CreatedHDC>
{
    HDC IHandle<HDC>.Handle => this;
    object? IHandle<HDC>.Wrapper => null;
    CreatedHDC IHandle<CreatedHDC>.Handle => this;
    object? IHandle<CreatedHDC>.Wrapper => null;

    public static implicit operator HDC(CreatedHDC hdc) => new(hdc.Value);
    public void Dispose() => Interop.DeleteDC(this);
}
