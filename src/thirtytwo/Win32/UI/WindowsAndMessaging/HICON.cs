// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe partial struct HICON : IHandle<HICON>, IDisposable
{
    HICON IHandle<HICON>.Handle => this;
    object? IHandle<HICON>.Wrapper => null;

    public static HICON Invalid => new(-1);

    public static implicit operator HICON(IconId id) => PInvoke.LoadIcon(default, (PCWSTR)(char*)(uint)id);
    public static implicit operator HANDLE(HICON handle) => (HANDLE)handle.Value;
    public static explicit operator HICON(HANDLE handle) => (HICON)handle.Value;

    public static HICON ExtractIcon(string file, int id, bool large = true)
    {
        HICON icon = default;
        HICON* largeIcon = large ? &icon : null;
        HICON* smallIcon = large ? null : &icon;
        fixed (char* filePath = file)
        {
            PInvoke.SHDefExtractIcon(new PCWSTR(filePath), id, 0, largeIcon, smallIcon, 0).ThrowOnFailure();
        }

        return icon;
    }

    public static HICON ExtractIcon(string file, int id, ushort size)
    {
        HICON icon = default;
        fixed (char* filePath = file)
        {
            PInvoke.SHDefExtractIcon(
                new PCWSTR(filePath),
                id,
                0,
                &icon,
                null,
                Conversion.HighLowToInt(size, size)).ThrowOnFailure();
        }

        return icon;
    }

    public static HICON ExtractIcon(SHSTOCKICONID id, ushort size = 0)
    {
        SHSTOCKICONINFO info = new()
        {
            cbSize = (uint)sizeof(SHSTOCKICONINFO)
        };

        PInvoke.SHGetStockIconInfo(id, SHGSI_FLAGS.SHGSI_ICONLOCATION, &info).ThrowOnFailure();

        HICON icon = default;
        PInvoke.SHDefExtractIcon(
            (PCWSTR)info.szPath.Value,
            info.iIcon,
            0,
            &icon,
            null,
            Conversion.HighLowToInt(size, size)).ThrowOnFailure();

        return icon;
    }

    public static int GetFileIconCount(string file)
    {
        uint result;
        fixed (char* filePath = file)
        {
            result = PInvoke.ExtractIconEx(new PCWSTR(filePath), -1, null, null, 0);
        }

        if (result == uint.MaxValue)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        return (int)result;
    }

    public void Dispose()
    {
        if (!IsNull)
        {
            PInvoke.DestroyIcon(this);
        }

        Unsafe.AsRef(in this) = default;
    }
}