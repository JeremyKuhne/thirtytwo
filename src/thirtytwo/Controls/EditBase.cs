// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public abstract class EditBase : CustomControl
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

    /// <summary>
    ///  Gets the current selection range.
    /// </summary>
    public (int Start, int End) GetSelection()
    {
        LRESULT result = this.SendMessage((MessageType)Interop.EM_GETSEL);
        return (result.LOWORD, result.HIWORD);
    }

    /// <summary>
    ///  Selects the given character range.
    /// </summary>
    public void SetSelection(int start, int end)
        => this.SendMessage((MessageType)Interop.EM_SETSEL, (WPARAM)start, (LPARAM)end);

    /// <summary>
    ///  Replaces the currently selected text.
    /// </summary>
    public unsafe void ReplaceSelection(string text, bool allowUndo = true)
    {
        fixed (char* c = text)
        {
            this.SendMessage((MessageType)Interop.EM_REPLACESEL, (WPARAM)(BOOL)allowUndo, (LPARAM)c);
        }
    }

    /// <summary>
    ///  Gets or sets the modified state of the control.
    /// </summary>
    public bool Modified
    {
        get => this.SendMessage((MessageType)Interop.EM_GETMODIFY) != 0;
        set => this.SendMessage((MessageType)Interop.EM_SETMODIFY, (WPARAM)(BOOL)value);
    }

    /// <summary>
    ///  Returns <see langword="true"/> if the control can undo the last action.
    /// </summary>
    public bool CanUndo => this.SendMessage((MessageType)Interop.EM_CANUNDO) != 0;

    /// <summary>
    ///  Undoes the last action, if possible.
    /// </summary>
    public bool Undo() => this.SendMessage((MessageType)Interop.EM_UNDO) != 0;

    /// <summary>
    ///  Clears the undo buffer.
    /// </summary>
    public void EmptyUndoBuffer() => this.SendMessage((MessageType)Interop.EM_EMPTYUNDOBUFFER);
}