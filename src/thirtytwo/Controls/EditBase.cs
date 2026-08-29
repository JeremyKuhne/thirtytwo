// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base wrapper for edit-style controls.
/// </summary>
public abstract class EditBase : RegisteredControl
{
    /// <summary>
    ///  Initializes a base edit control wrapper.
    /// </summary>
    /// <param name="bounds">The control bounds in parent client coordinates.</param>
    /// <param name="windowClass">The window class used to create the control.</param>
    /// <param name="style">The combined base and edit style flags.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="extendedStyle">The extended window style flags.</param>
    /// <param name="parentWindow">The parent window that owns this control.</param>
    /// <param name="parameters">Additional creation parameters passed as <c>lpParam</c>.</param>
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
    public int LineCount => (int)this.SendMessage((MessageType)PInvoke.EM_GETLINECOUNT);

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
    /// <param name="lineNumber">The zero-based line index.</param>
    /// <returns>The requested line text, or an empty string when the line does not exist.</returns>
    public unsafe string GetLine(int lineNumber)
    {
        int index = (int)this.SendMessage((MessageType)PInvoke.EM_LINEINDEX, (WPARAM)lineNumber);
        if (index < 0)
        {
            return string.Empty;
        }

        int lineLength = (int)this.SendMessage((MessageType)PInvoke.EM_LINELENGTH, (WPARAM)index);

        if (lineLength == 0)
        {
            return string.Empty;
        }

        using var buffer = new BufferScope<char>(stackalloc char[256], lineLength);

        fixed (char* c = buffer)
        {
            *(ushort*)c = (ushort)buffer.Length;
            int copied = (int)this.SendMessage((MessageType)PInvoke.EM_GETLINE, (WPARAM)lineNumber, (LPARAM)c);
            return buffer[..copied].ToString();
        }
    }

    /// <summary>
    ///  Gets the current selection range.
    /// </summary>
    /// <returns>A tuple containing the inclusive start and exclusive end character positions.</returns>
    public (int Start, int End) GetSelection()
    {
        LRESULT result = this.SendMessage((MessageType)PInvoke.EM_GETSEL);
        return (result.LOWORD, result.HIWORD);
    }

    /// <summary>
    ///  Selects the given character range.
    /// </summary>
    /// <param name="start">The starting character position.</param>
    /// <param name="end">The ending character position.</param>
    public void SetSelection(int start, int end)
        => this.SendMessage((MessageType)PInvoke.EM_SETSEL, (WPARAM)start, (LPARAM)end);

    /// <summary>
    ///  Replaces the currently selected text.
    /// </summary>
    /// <param name="text">The replacement text.</param>
    /// <param name="allowUndo"><see langword="true"/> to add the operation to the undo stack.</param>
    public unsafe void ReplaceSelection(string text, bool allowUndo = true)
    {
        fixed (char* c = text)
        {
            this.SendMessage((MessageType)PInvoke.EM_REPLACESEL, (WPARAM)(BOOL)allowUndo, (LPARAM)c);
        }
    }

    /// <summary>
    ///  Gets or sets the modified state of the control.
    /// </summary>
    public bool Modified
    {
        get => this.SendMessage((MessageType)PInvoke.EM_GETMODIFY) != 0;
        set => this.SendMessage((MessageType)PInvoke.EM_SETMODIFY, (WPARAM)(BOOL)value);
    }

    /// <summary>
    ///  Returns <see langword="true"/> if the control can undo the last action.
    /// </summary>
    public bool CanUndo => this.SendMessage((MessageType)PInvoke.EM_CANUNDO) != 0;

    /// <summary>
    ///  Undoes the last action, if possible.
    /// </summary>
    /// <returns><see langword="true"/> if the control performed an undo operation.</returns>
    public bool Undo() => this.SendMessage((MessageType)PInvoke.EM_UNDO) != 0;

    /// <summary>
    ///  Clears the undo buffer.
    /// </summary>
    public void EmptyUndoBuffer() => this.SendMessage((MessageType)PInvoke.EM_EMPTYUNDOBUFFER);
}