// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI bitmap handle (<c>HBITMAP</c>).
/// </summary>
public partial struct HBITMAP : IHandle<HBITMAP>
{
    /// <inheritdoc cref="IHandle{T}.Handle"/>
    HBITMAP IHandle<HBITMAP>.Handle => this;

    /// <inheritdoc cref="IHandle{T}.Wrapper"/>
    object? IHandle<HBITMAP>.Wrapper => null;
}