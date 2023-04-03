// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public static unsafe class IconExtensions
{
    public static ICONINFOEXW GetInfo<T>(this T icon)
        where T : IHandle<HICON>
    {
        ICONINFOEXW info = new()
        {
            cbSize = (uint)sizeof(ICONINFOEXW)
        };

        Interop.GetIconInfoEx(icon.Handle, &info).ThrowLastErrorIfFalse();

        GC.KeepAlive(icon.Wrapper);
        return info;
    }

    public static Size GetSize<T>(this T icon)
        where T : IHandle<HICON>
    {
        ICONINFO info = default;
        Interop.GetIconInfo(icon.Handle, &info).ThrowLastErrorIfFalse();
        HBITMAP handle = info.hbmColor.IsNull ? info.hbmMask : info.hbmColor;
        BITMAP bitmap;
        if (Interop.GetObject(handle, sizeof(BITMAP), &bitmap) == 0)
        {
            throw new InvalidOperationException();
        }

        GC.KeepAlive(icon.Wrapper);
        return new(bitmap.bmWidth, bitmap.bmHeight);
    }

    public static HICON Copy<T>(this T icon, ushort newSize)
        where T : IHandle<HICON>
    {
        HICON newIcon = (HICON)Interop.CopyImage(icon.Handle, GDI_IMAGE_TYPE.IMAGE_ICON, newSize, newSize, IMAGE_FLAGS.LR_COPYFROMRESOURCE);
        if (newIcon.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(icon.Wrapper);
        return newIcon;
    }
}