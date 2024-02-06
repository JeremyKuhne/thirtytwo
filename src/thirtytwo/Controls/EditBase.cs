// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public abstract class EditBase : Control
{
    protected EditBase(
        Rectangle bounds,
        WindowClass windowClass,
        WindowStyles style,
        string? text = default,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            text,
            style,
            extendedStyle,
            parentWindow,
            windowClass,
            parameters)
    {
    }

    /// <summary>
    ///  Gets the total number of lines for the control. Never less than 1.
    /// </summary>
    public int LineCount => (int)this.SendMessage((MessageType)Interop.EM_GETLINECOUNT);

    /// <summary>
    ///  Gets the text for the specified <paramref name="lineNumber"/>. If the line doesn't exist, returns
    ///  an empty <see langword="string"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   For single-line edit controls this always returns the entire text of the control. RichEdit controls
    ///   will still listen to the <paramref name="lineNumber"/>, however.
    ///  </para>
    /// </remarks>
    public unsafe string GetLine(int lineNumber)
    {
        int index = (int)this.SendMessage((MessageType)Interop.EM_LINEINDEX, (WPARAM)lineNumber);
        if (index < 0)
        {
            return string.Empty;
        }

        int lineLength = (int)this.SendMessage((MessageType)Interop.EM_LINELENGTH, (WPARAM)index);

        if (lineLength == 0)
        {
            return string.Empty;
        }

        using var buffer = new BufferScope<char>(stackalloc char[256], lineLength);

        fixed (char* c = buffer)
        {
            *(ushort*)c = (ushort)buffer.Length;
            int copied = (int)this.SendMessage((MessageType)Interop.EM_GETLINE, (WPARAM)lineNumber, (LPARAM)c);
            return buffer[..copied].ToString();
        }
    }
}