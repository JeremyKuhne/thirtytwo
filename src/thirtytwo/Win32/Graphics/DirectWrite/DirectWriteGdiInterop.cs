// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <devdoc>
///  https://learn.microsoft.com/windows/win32/directwrite/appendix--win32-migration
///  https://learn.microsoft.com/windows/win32/directwrite/interoperating-with-gdi
/// </devdoc>
public unsafe class DirectWriteGdiInterop : DirectDrawBase<IDWriteGdiInterop>
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