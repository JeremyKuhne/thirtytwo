// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Object that allows interoperation between GDI and DirectWrite. Primarily used to convert
///  between GDI logical fonts and DirectWrite text formats. Also allows creating a
///  <see cref="IDWriteBitmapRenderTarget"/> from an <see cref="HDC"/>.
/// </summary>
/// <devdoc>
///  https://learn.microsoft.com/windows/win32/directwrite/appendix--win32-migration
///  https://learn.microsoft.com/windows/win32/directwrite/interoperating-with-gdi
/// </devdoc>
public unsafe sealed class DirectWriteGdiInterop : DirectDrawBase<IDWriteGdiInterop>
{
    public DirectWriteGdiInterop() : this(Create())
    {
    }

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