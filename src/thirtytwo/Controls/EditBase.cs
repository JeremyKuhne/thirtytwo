// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public abstract class EditBase : RegisteredControl
{
    private bool _dropInsertionCaretVisible;
    private long _textGeneration;

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

    /// <summary>Gets or sets whether Unicode text can be dropped into this control.</summary>
    /// <remarks>
    ///  A drop replaces the current selection, or inserts at the caret when the selection is empty. The drop is
    ///  accepted when the source permits moving or copying. This native control path uses classic OLE registration
    ///  on the control HWND.
    /// </remarks>
    public bool EnableDrop
    {
        get => IsTextDropEnabled;
        set => SetTextDropEnabled(
            value,
            ReplaceSelectionWithDroppedText,
            GetSelection,
            SetSelection,
            GetDropInsertionIndex,
            () => Text.Length,
            focusForInsertion: true,
            beginInsertion: BeginDropInsertion,
            cancelInsertion: EndDropInsertion,
            commitInsertion: EndDropInsertion);
    }

    /// <summary>Gets or sets whether text can be dragged from this control.</summary>
    /// <remarks>
    ///  A drag can start only within selected text. Moving is requested by default; hold Ctrl to request copying.
    ///  An empty selection cannot be dragged. This native control path uses classic OLE <c>DoDragDrop</c>.
    /// </remarks>
    public bool EnableDrag
    {
        get => IsTextDragEnabled;
        set => SetTextDragEnabled(
            value,
            GetTextForDrag,
            () => _textGeneration,
            GetSelection,
            SetSelection,
            ReplaceSelectionWithDroppedText,
            GetCharacterIndexFromPoint,
            CanStartTextDrag);
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
    public (int Start, int End) GetSelection()
    {
        LRESULT result = this.SendMessage((MessageType)PInvoke.EM_GETSEL);
        return (result.LOWORD, result.HIWORD);
    }

    /// <summary>
    ///  Selects the given character range.
    /// </summary>
    public void SetSelection(int start, int end)
        => this.SendMessage((MessageType)PInvoke.EM_SETSEL, (WPARAM)start, (LPARAM)end);

    /// <summary>
    ///  Replaces the currently selected text.
    /// </summary>
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
    public bool Undo() => this.SendMessage((MessageType)PInvoke.EM_UNDO) != 0;

    /// <summary>
    ///  Clears the undo buffer.
    /// </summary>
    public void EmptyUndoBuffer() => this.SendMessage((MessageType)PInvoke.EM_EMPTYUNDOBUFFER);

    private protected virtual string GetTextForDrag()
    {
        string text = Text;
        (int start, int end) = GetSelection();
        return end > start && start >= 0 && end <= text.Length
            ? text[start..end]
            : string.Empty;
    }

    protected override void OnCommand(int controlId, int notificationCode)
    {
        if (notificationCode == PInvoke.EN_CHANGE)
        {
            _textGeneration++;
        }

        base.OnCommand(controlId, notificationCode);
    }

    private protected virtual int GetCharacterIndexFromPoint(Point position)
    {
        LPARAM packedPosition = (LPARAM)(nint)(
            (uint)(ushort)position.X | ((uint)(ushort)position.Y << 16));
        return this.SendMessage((MessageType)PInvoke.EM_CHARFROMPOS, lParam: packedPosition).LOWORD;
    }

    private protected virtual Point GetCharacterPosition(int index)
    {
        LRESULT result = this.SendMessage((MessageType)PInvoke.EM_POSFROMCHAR, (WPARAM)index);
        return new Point(unchecked((short)result.LOWORD), unchecked((short)result.HIWORD));
    }

    private protected virtual unsafe void BeginDropInsertion()
    {
        using DeviceContext context = this.GetDeviceContext();
        using var fontScope = context.SelectObject(this.GetFontHandle());
        TEXTMETRICW metrics;
        if (!PInvoke.GetTextMetrics(context, &metrics))
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        if (!PInvoke.CreateCaret(Handle, default, 0, metrics.tmHeight))
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }
    }

    private protected virtual void UpdateDropInsertion(int index)
    {
        Point point = GetCharacterPosition(index);
        if (!PInvoke.SetCaretPos(point.X, point.Y))
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        if (!_dropInsertionCaretVisible)
        {
            if (!PInvoke.ShowCaret(Handle))
            {
                Error.GetLastError().ThrowThirtyTwoException();
            }

            _dropInsertionCaretVisible = true;
        }

    }

    private protected virtual void EndDropInsertion()
    {
        if (_dropInsertionCaretVisible)
        {
            _ = PInvoke.HideCaret(Handle);
            _dropInsertionCaretVisible = false;
        }
    }

    private bool CanStartTextDrag(Point position)
    {
        (int start, int end) = GetSelection();
        if (end <= start)
        {
            return false;
        }

        int characterIndex = GetCharacterIndexFromPoint(position);
        return characterIndex >= start && characterIndex < end;
    }

    private int GetDropInsertionIndex(Point screenLocation)
    {
        if (!this.ScreenToClient(ref screenLocation))
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        int index = GetCharacterIndexFromPoint(screenLocation);
        UpdateDropInsertion(index);
        return index;
    }

    private void ReplaceSelectionWithDroppedText(string text) => ReplaceSelection(text);
}
