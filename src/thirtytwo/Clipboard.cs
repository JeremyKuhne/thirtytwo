// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Ole;

namespace Windows;

/// <summary>
///  Clipboard manipulation routines.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://devblogs.microsoft.com/oldnewthing/20210526-00/?p=105252">
///    How ownership of the Windows clipboard is tracked in Win32.
///   </see>
///  </para>
/// </remarks>
public unsafe class Clipboard
{
    /// <summary>
    ///  Gets the formats that are currently available on the clipboard.
    /// </summary>
    public static unsafe uint[] GetAvailableClipboardFormats()
    {
        uint count = (uint)PInvoke.CountClipboardFormats();
        if (count == 0)
        {
            return [];
        }

        using BufferScope<uint> buffer = new(stackalloc uint[(int)count]);
        do
        {
            if (PInvoke.GetUpdatedClipboardFormats(buffer, out count))
            {
                break;
            }

            WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER.ThrowIfLastErrorNot();
            buffer.EnsureCapacity((int)count);
        } while (true);

        return count == 0 ? [] : buffer[..(int)count].ToArray();
    }

    /// <summary>
    ///  Returns true if the requested format is available.
    /// </summary>
    public static bool IsClipboardFormatAvailable(uint format)
    {
        bool result = PInvoke.IsClipboardFormatAvailable(format);

        if (!result)
        {
            WIN32_ERROR.NO_ERROR.ThrowIfLastErrorNot();
        }

        return result;
    }

    /// <summary>
    ///  Returns true if there is text on the clipboard.
    /// </summary>
    public static bool IsClipboardTextAvailable()
        => PInvoke.IsClipboardFormatAvailable((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);

    /// <summary>
    ///  Empty the clipboard.
    /// </summary>
    public static void EmptyClipboard() => PInvoke.EmptyClipboard().ThrowLastErrorIfFalse();

    /// <summary>
    ///  This only works for types that aren't built in (e.g. defined in ClipboardFormat).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if passing in a built-in format type.</exception>
    public static string GetClipboardFormatName(uint format)
    {
        using BufferScope<char> buffer = new(stackalloc char[128]);

        while (true)
        {
            fixed (char* b = buffer)
            {
                int count = PInvoke.GetClipboardFormatName(format, b, buffer.Length);
                if (count == 0)
                {
                    Error.GetLastError().ThrowThirtyTwoException();
                }

                if (count > buffer.Length - 2)
                {
                    buffer.EnsureCapacity(buffer.Length * 2);
                    continue;
                }

                return buffer.Slice(0, count).ToString();
            }
        }
    }

    /// <summary>
    ///  Registers the given format if not already registered. Returns the format id.
    /// </summary>
    public static uint RegisterClipboardFormat(string formatName)
    {
        uint id = PInvoke.RegisterClipboardFormat(formatName);
        if (id == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException(formatName);
        }

        return id;
    }

    /// <inheritdoc cref="SetClipboardData{T}(ReadOnlySpan{T}, string)"/>
    public static unsafe void SetClipboardData<T>(ReadOnlySpan<T> data, uint format)
        where T : unmanaged
    {
        HGLOBAL global = PInvoke.GlobalAlloc(
            GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE,
            (nuint)((data.Length + 1) * sizeof(T)));

        Span<T> buffer = new(PInvoke.GlobalLock(global), data.Length + 1);
        data.CopyTo(buffer);
        buffer[^1] = default;

        PInvoke.GlobalUnlock(global);
        PInvoke.SetClipboardData(format, (HANDLE)(nint)global);
    }

    /// <summary>
    ///  Gets text from the clipboard if it is available.
    /// </summary>
    /// <returns>Returns the clipboard text or <see langword="null"/> if text is not available.</returns>
    public static string? GetClipboardText()
    {
        if (!IsClipboardTextAvailable() || !OpenClipboard())
        {
            return null;
        }

        HGLOBAL global = default;

        try
        {
            global = (HGLOBAL)(nint)PInvoke.GetClipboardData((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);
            if (global == 0)
            {
                return null;
            }

            int size = checked((int)(PInvoke.GlobalSize(global) / sizeof(char)));

            if (size == 0)
            {
                return string.Empty;
            }

            ReadOnlySpan<char> buffer = new(PInvoke.GlobalLock(global), size);
            return Touki.SpanExtensions.SliceAtNull(buffer).ToString();
        }
        finally
        {
            if (global != 0)
            {
                PInvoke.GlobalUnlock(global);
            }

            CloseClipboard();
        }
    }

    /// <summary>
    ///  Places data on the clipboard in the specified format.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   It is unnecessary to put more than one text or bitmap format as Windows will allow retrieving in any format.
    ///   See <see href="https://learn.microsoft.com/windows/win32/dataxchg/clipboard-formats#synthesized-clipboard-formats">
    ///   Synthesized Clipboard Formats</see>.
    ///  </para>
    /// </remarks>
    public static void SetClipboardData<T>(ReadOnlySpan<T> data, string format) where T : unmanaged
        => SetClipboardData(data, RegisterClipboardFormat(format));

    /// <summary>
    ///  Sets the given text on the clipboard.
    /// </summary>
    public static void SetClipboardText(ReadOnlySpan<char> text)
        => SetClipboardData(text, (uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);

    private static bool OpenClipboardInternal(HWND hwnd)
    {
        if (!PInvoke.OpenClipboard(hwnd))
        {
            WIN32_ERROR error = Error.GetLastError();
            if (error == WIN32_ERROR.ERROR_ACCESS_DENIED)
            {
                // Clipboard is already open.
                return false;
            }

            error.ThrowThirtyTwoException();
        }

        return true;
    }

    /// <inheritdoc cref="OpenClipboard{T}(T)"/>
    public static bool OpenClipboard() => OpenClipboardInternal(default);

    /// <summary>
    ///  Open the clipboard for modification.
    /// </summary>
    /// <param name="owner">The window that will get render format messages when using delay rendering.</param>
    /// <returns><see langword="false"/> if already opened.</returns>
    public static bool OpenClipboard<T>(T owner) where T : IHandle<HWND>
    {
        bool result = OpenClipboardInternal(owner.Handle);
        GC.KeepAlive(owner.Wrapper);
        return result;
    }

    /// <summary>
    ///  Closes the clipboard so other threads can access it.
    /// </summary>
    public static void CloseClipboard()
    {
        if (!PInvoke.CloseClipboard())
        {
            WIN32_ERROR error = Error.GetLastError();
            if (error == WIN32_ERROR.ERROR_CLIPBOARD_NOT_OPEN)
            {
                // Clipboard isn't open. We won't throw here as it is pretty common to fail to open the
                // clipboard (as some other window may have it open).
                return;
            }

            error.ThrowThirtyTwoException();
        }
    }

    /// <summary>
    ///  Returns the window handle that has the clipboard open, if any.
    /// </summary>
    public static HWND GetOpenClipboardWindow() => PInvoke.GetOpenClipboardWindow();

    /// <summary>
    ///  Returns the window handle that owns the clipboard data, if any.
    /// </summary>
    public static HWND GetClipboardOwner() => PInvoke.GetClipboardOwner();

    /// <summary>
    ///  Registers the given window to get <see cref="MessageType.ClipboardUpdate"/> messages when the
    ///  clipboard content changes.
    /// </summary>
    public static void AddClipboardFormatListener<T>(T window) where T : IHandle<HWND>
    {
        PInvoke.AddClipboardFormatListener(window.Handle).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }

    /// <summary>
    ///  Unregisters the given window from getting <see cref="MessageType.ClipboardUpdate"/> messages.
    /// </summary>
    public static void RemoveClipboardFormatListener<T>(T window) where T : IHandle<HWND>
    {
        PInvoke.RemoveClipboardFormatListener(window.Handle).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }
}