// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

/// <summary>
///  Provides Win32 icon helper methods for handle wrappers.
/// </summary>
public static unsafe class IconExtensions
{
    /// <summary>
    ///  Gets extended icon metadata from USER32.
    /// </summary>
    /// <typeparam name="T">The icon handle-wrapper type.</typeparam>
    /// <param name="icon">The icon handle wrapper.</param>
    /// <returns>The populated <c>ICONINFOEXW</c> structure for the icon.</returns>
    public static ICONINFOEXW GetInfo<T>(this T icon)
        where T : IHandle<HICON>
    {
        ICONINFOEXW info = new()
        {
            cbSize = (uint)sizeof(ICONINFOEXW)
        };

        PInvoke.GetIconInfoEx(icon.Handle, &info).ThrowLastErrorIfFalse();

        GC.KeepAlive(icon.Wrapper);
        return info;
    }

    /// <summary>
    ///  Gets the icon bitmap size in pixels.
    /// </summary>
    /// <typeparam name="T">The icon handle-wrapper type.</typeparam>
    /// <param name="icon">The icon handle wrapper.</param>
    /// <returns>The icon dimensions in physical pixels.</returns>
    public static Size GetSize<T>(this T icon)
        where T : IHandle<HICON>
    {
        ICONINFO info = default;
        PInvoke.GetIconInfo(icon.Handle, &info).ThrowLastErrorIfFalse();
        HBITMAP handle = info.hbmColor.IsNull ? info.hbmMask : info.hbmColor;
        BITMAP bitmap;
        if (PInvoke.GetObject(handle, sizeof(BITMAP), &bitmap) == 0)
        {
            throw new InvalidOperationException();
        }

        GC.KeepAlive(icon.Wrapper);
        return new(bitmap.bmWidth, bitmap.bmHeight);
    }

    /// <summary>
    ///  Creates a copied icon at the requested resource size.
    /// </summary>
    /// <typeparam name="T">The icon handle-wrapper type.</typeparam>
    /// <param name="icon">The source icon handle wrapper.</param>
    /// <param name="newSize">The requested width and height, in pixels.</param>
    /// <returns>A new icon handle that the caller owns.</returns>
    public static HICON Copy<T>(this T icon, ushort newSize)
        where T : IHandle<HICON>
    {
        HICON newIcon = (HICON)PInvoke.CopyImage(icon.Handle, GDI_IMAGE_TYPE.IMAGE_ICON, newSize, newSize, IMAGE_FLAGS.LR_COPYFROMRESOURCE);
        if (newIcon.IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(icon.Wrapper);
        return newIcon;
    }
}