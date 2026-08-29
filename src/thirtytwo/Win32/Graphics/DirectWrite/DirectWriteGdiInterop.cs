// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Object that allows interoperation between GDI and DirectWrite. Primarily used to convert
///  between GDI logical fonts and DirectWrite text formats. Also allows creating a
///  <see cref="IDWriteBitmapRenderTarget"/> from an <see cref="HDC"/>.
/// </summary>
/// <remarks>
///  Instances own the underlying <see cref="IDWriteGdiInterop"/> COM interface pointer and release it when disposed.
/// </remarks>
/// <devdoc>
///  https://learn.microsoft.com/windows/win32/directwrite/appendix--win32-migration
///  https://learn.microsoft.com/windows/win32/directwrite/interoperating-with-gdi
/// </devdoc>
public unsafe sealed class DirectWriteGdiInterop : DirectDrawBase<IDWriteGdiInterop>
{
    /// <summary>
    ///  Creates a GDI interop wrapper from <see cref="Application.DirectWriteFactory"/>.
    /// </summary>
    /// <remarks>
    ///  This constructor calls <see cref="IDWriteFactory.GetGdiInterop(IDWriteGdiInterop**)"/> and throws
    ///  when the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public DirectWriteGdiInterop() : this(Create())
    {
    }

    /// <summary>
    ///  Wraps an existing DirectWrite GDI interop pointer.
    /// </summary>
    /// <param name="gdiInterop">The existing interop interface pointer to wrap.</param>
    /// <remarks>The wrapper takes responsibility for releasing this COM interface pointer when disposed.</remarks>
    public DirectWriteGdiInterop(IDWriteGdiInterop* gdiInterop) : base(gdiInterop)
    {
    }

    private static IDWriteGdiInterop* Create()
    {
        IDWriteGdiInterop* format;
        Application.DirectWriteFactory.Pointer->GetGdiInterop(&format).ThrowOnFailure();
        return format;
    }
}