// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe partial struct HICON : IHandle<HICON>, IDisposable
{
    HICON IHandle<HICON>.Handle => this;
    object? IHandle<HICON>.Wrapper => null;

    public static HICON Invalid => new(-1);

    public static implicit operator HICON(IconId id) => Interop.LoadIcon(default, (PCWSTR)(char*)(uint)id);
    public static implicit operator HANDLE(HICON handle) => (HANDLE)handle.Value;
    public static explicit operator HICON(HANDLE handle) => (HICON)handle.Value;

    public static HICON ExtractIcon(string file, int id, bool large = true)
    {
        HICON icon = default;
        HRESULT result = Interop.SHDefExtractIcon(file, id, 0, large ? &icon : null, large ? null : &icon, 0);
        result.ThrowOnFailure();
        return icon;
    }

    public static HICON ExtractIcon(string file, int id, ushort size)
    {
        HICON icon = default;
        HRESULT result = Interop.SHDefExtractIcon(file, id, 0, &icon, null, Conversion.HighLowToInt(size, size));
        result.ThrowOnFailure();
        return icon;
    }

    public static HICON ExtractIcon(SHSTOCKICONID id, ushort size = 0)
    {
        SHSTOCKICONINFO info = new()
        {
            cbSize = (uint)sizeof(SHSTOCKICONINFO)
        };

        Interop.SHGetStockIconInfo(id, SHGSI_FLAGS.SHGSI_ICONLOCATION, &info).ThrowOnFailure();

        HICON icon = default;
        Interop.SHDefExtractIcon(
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
        uint result = Interop.ExtractIconEx(file, -1, null, null, 0);
        if (result == uint.MaxValue)
        {
            Error.ThrowLastError();
        }

        return (int)result;
    }

    public void Dispose()
    {
        if (!IsNull)
        {
            Interop.DestroyIcon(this);
        }
    }
}