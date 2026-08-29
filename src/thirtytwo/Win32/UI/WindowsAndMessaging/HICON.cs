// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
///  Icon handle wrapper.
/// </summary>
public unsafe partial struct HICON : IHandle<HICON>, IDisposable
{
    /// <summary>
    ///  Gets this handle value for <see cref="IHandle{THandle}"/>.
    /// </summary>
    HICON IHandle<HICON>.Handle => this;

    /// <summary>
    ///  Gets the optional managed wrapper for <see cref="IHandle{THandle}"/>.
    /// </summary>
    object? IHandle<HICON>.Wrapper => null;

    /// <summary>
    ///  Gets the sentinel icon value represented by <c>-1</c>.
    /// </summary>
    public static HICON Invalid => new(-1);

    /// <summary>
    ///  Loads an icon resource by identifier.
    /// </summary>
    /// <param name="id">System icon resource identifier.</param>
    /// <returns>The loaded icon handle.</returns>
    public static implicit operator HICON(IconId id) => PInvoke.LoadIcon(default, (PCWSTR)(char*)(uint)id);

    /// <summary>
    ///  Converts an icon handle to a generic HANDLE.
    /// </summary>
    /// <param name="handle">Icon handle to convert.</param>
    /// <returns>The corresponding HANDLE value.</returns>
    public static implicit operator HANDLE(HICON handle) => (HANDLE)handle.Value;

    /// <summary>
    ///  Converts a generic HANDLE to an icon handle.
    /// </summary>
    /// <param name="handle">Handle value to reinterpret as <see cref="HICON"/>.</param>
    /// <returns>The corresponding icon handle.</returns>
    public static explicit operator HICON(HANDLE handle) => (HICON)handle.Value;

    /// <summary>
    ///  Extracts an icon from a file by index.
    /// </summary>
    /// <param name="file">Source file path.</param>
    /// <param name="id">Icon index in the file.</param>
    /// <param name="large">
    ///  <see langword="true"/> to request the large icon slot; otherwise the small icon slot.
    /// </param>
    /// <returns>The extracted icon handle.</returns>
    /// <remarks>
    ///  <para>
    ///   Failure HRESULT values are converted to exceptions via <c>ThrowOnFailure()</c>.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Extracts an icon from a file by index and requested size.
    /// </summary>
    /// <param name="file">Source file path.</param>
    /// <param name="id">Icon index in the file.</param>
    /// <param name="size">Requested width and height in pixels.</param>
    /// <returns>The extracted icon handle.</returns>
    /// <remarks>
    ///  <para>
    ///   The requested size is passed as a combined high/low integer value.
    ///  </para>
    ///  <para>
    ///   Failure HRESULT values are converted to exceptions via <c>ThrowOnFailure()</c>.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Extracts a stock icon by stock identifier.
    /// </summary>
    /// <param name="id">Stock icon identifier.</param>
    /// <param name="size">Requested width and height in pixels.</param>
    /// <returns>The extracted icon handle.</returns>
    /// <remarks>
    ///  <para>
    ///   The icon location is first resolved with <c>SHGetStockIconInfo</c>, then extracted via
    ///   <c>SHDefExtractIcon</c>.
    ///  </para>
    ///  <para>
    ///   Failure HRESULT values are converted to exceptions via <c>ThrowOnFailure()</c>.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Gets the icon count stored in the specified file.
    /// </summary>
    /// <param name="file">Source file path.</param>
    /// <returns>The number of icons reported by <c>ExtractIconEx</c>.</returns>
    /// <remarks>
    ///  <para>
    ///   A native return value of <see cref="uint.MaxValue"/> is treated as a sentinel failure
    ///   and converted to an exception from <c>GetLastError</c>.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Destroys the icon when the handle is non-null and clears this instance.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This method always calls <c>DestroyIcon</c> for non-null values, so callers must only
    ///   dispose handles they intend to destroy.
    ///  </para>
    /// </remarks>
    public void Dispose()
    {
        if (!IsNull)
        {
            PInvoke.DestroyIcon(this);
        }

        Unsafe.AsRef(in this) = default;
    }
}