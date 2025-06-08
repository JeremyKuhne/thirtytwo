// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Win32 ComboBox control wrapper.
/// </summary>
public unsafe partial class ComboBoxControl : RegisteredControl
{
    private static readonly WindowClass s_comboBoxClass = new("ComboBox");

    /// <summary>
    ///  Occurs when the selection in the ComboBox is changed by the user.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Occurs when the ComboBox receives the keyboard focus.
    /// </summary>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-setfocus</docs>
    public event EventHandler? GotFocus;

    /// <summary>
    ///  Occurs when the ComboBox loses the keyboard focus.
    /// </summary>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-killfocus</docs>
    public event EventHandler? LostFocus;

    /// <summary>
    ///  Occurs when the text in the ComboBox's edit portion is changed.
    /// </summary>
    /// <remarks>
    ///  This event is not raised for ComboBoxes created with <see cref="Styles.DropDownList"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-editchange</docs>
    public event EventHandler? TextChanged;

    /// <summary>
    ///  Occurs when the text in the ComboBox's edit portion is about to update.
    /// </summary>
    /// <remarks>
    ///  This event is not raised for ComboBoxes created with <see cref="Styles.DropDownList"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-editupdate</docs>
    public event EventHandler? TextUpdated;

    /// <summary>
    ///  Occurs when the ComboBox's drop-down list is about to be displayed.
    /// </summary>
    /// <remarks>
    ///  This event does not occur for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-dropdown</docs>
    public event EventHandler? DropDown;

    /// <summary>
    ///  Occurs when the ComboBox's drop-down list is closed.
    /// </summary>
    /// <remarks>
    ///  This event does not occur for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-closeup</docs>
    public event EventHandler? CloseUp;

    /// <summary>
    ///  Occurs when the user double-clicks an item in the ComboBox's list.
    /// </summary>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-dblclk</docs>
    public event EventHandler? DoubleClicked;

    /// <summary>
    ///  Occurs when the current selection is finalized by the user.
    /// </summary>
    /// <remarks>
    ///  This event does not occur for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-selendok</docs>
    public event EventHandler? SelectionCommitted;

    /// <summary>
    ///  Occurs when the user cancels a selection in the drop-down list.
    /// </summary>
    /// <remarks>
    ///  This event does not occur for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    /// <docs>https://learn.microsoft.com/windows/win32/controls/cbn-selendcancel</docs>
    public event EventHandler? SelectionCanceled;

    public ComboBoxControl(
        Rectangle bounds = default,
        string? text = default,
        Styles comboBoxStyle = Styles.DropDown,
        WindowStyles style = WindowStyles.Overlapped | WindowStyles.Child | WindowStyles.Visible,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default)
        : base(
            bounds,
            text,
            style | (WindowStyles)(uint)comboBoxStyle,
            extendedStyle,
            parentWindow,
            s_comboBoxClass,
            parameters)
    {
    }

    /// <summary>
    ///  Returns the number of items in the ComboBox.
    /// </summary>
    public int Count => (int)this.SendMessage((MessageType)Interop.CB_GETCOUNT);

    /// <summary>
    ///  Adds a new item to the ComboBox. The item is added to the end of the list.
    /// </summary>
    /// <returns>The count of items in the ComboBox.</returns>
    /// <exception cref="OutOfMemoryException">The ComboBox was unable to allocate space.</exception>
    public int AddItem(string item)
    {
        ArgumentException.ThrowIfNullOrEmpty(item);

        fixed (char* pItem = item)
        {
            int index = (int)this.SendMessage((MessageType)Interop.CB_ADDSTRING, 0, (LPARAM)pItem);
            if (index == Interop.CB_ERRSPACE)
            {
                throw new OutOfMemoryException();
            }

            return index + 1;
        }
    }

    /// <summary>
    ///  Adds a range of items.
    /// </summary>
    /// <returns>The count of items in the ComboBox.</returns>
    /// <exception cref="OutOfMemoryException">The ComboBox was unable to allocate space.</exception>
    public int AddItems(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        int count = 0;
        int bytes = 0;

        IEnumerator<string> enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
        {
            count++;
            bytes += sizeof(char) * (enumerator.Current.Length + 1);
        }

        if (count == 0)
        {
            return Count;
        }

        if (this.SendMessage((MessageType)Interop.CB_INITSTORAGE, (WPARAM)count, (LPARAM)bytes) == Interop.CB_ERRSPACE)
        {
            throw new OutOfMemoryException();
        }

        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            count = AddItem(enumerator.Current);
        }

        return count;
    }

    /// <summary>
    ///  Removes the item at the specified index. The index is zero-based.
    /// </summary>
    /// <returns>The count of items remaining in the ComboBox.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was not valid.</exception>
    public int RemoveItem(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);

        int result = (int)this.SendMessage((MessageType)Interop.CB_DELETESTRING, (WPARAM)index);
        if (result == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return result;
    }

    /// <summary>
    ///  Removes all items from the ComboBox.
    /// </summary>
    public void Clear() => this.SendMessage((MessageType)Interop.CB_RESETCONTENT);

    /// <summary>
    ///  Changes the selected index of the ComboBox. If the index is -1, no item is selected.
    /// </summary>
    public int SelectedIndex
    {
        get => (int)this.SendMessage((MessageType)Interop.CB_GETCURSEL);
        set => this.SendMessage((MessageType)Interop.CB_SETCURSEL, (WPARAM)value);
    }

    /// <summary>
    ///  Gets the text of the item at the specified index. The index is zero-based.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was invalid.</exception>
    public string GetItemText(int index)
    {
        BufferScope<char> buffer = new(stackalloc char[128]);
        string result = GetItemText(ref buffer, index).ToString();
        buffer.Dispose();
        return result;
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was invalid.</exception>
    private ReadOnlySpan<char> GetItemText(ref BufferScope<char> buffer, int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        int length = (int)this.SendMessage((MessageType)Interop.CB_GETLBTEXTLEN, (WPARAM)index);
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        buffer.EnsureCapacity(length + 1);
        fixed (char* pBuffer = buffer)
        {
            length = (int)this.SendMessage((MessageType)Interop.CB_GETLBTEXT, (WPARAM)index, pBuffer);
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return buffer[..length];
    }

    /// <summary>
    ///  Gets or sets the selected item in the ComboBox by label. If the item is not found, the index is set to -1.
    /// </summary>
    public string? SelectedItem
    {
        get
        {
            int index = SelectedIndex;
            if (index < 0)
            {
                return null;
            }

            return GetItemText(index);
        }
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }

            fixed (char* pValue = value)
            {
                SelectedIndex = (int)this.SendMessage((MessageType)Interop.CB_FINDSTRINGEXACT, (WPARAM)(-1), (LPARAM)pValue);
            }
        }
    }

    protected override void OnCommand(int controlId, int notificationCode)
    {
        switch ((uint)notificationCode)
        {
            case Interop.CBN_SELCHANGE:
                OnSelectionChange();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_SETFOCUS:
                OnGotFocus();
                GotFocus?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_KILLFOCUS:
                OnLostFocus();
                LostFocus?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_DROPDOWN:
                OnDropDown();
                DropDown?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_CLOSEUP:
                OnCloseUp();
                CloseUp?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_DBLCLK:
                OnDoubleClicked();
                DoubleClicked?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_EDITCHANGE:
                OnTextChanged();
                TextChanged?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_EDITUPDATE:
                OnTextUpdated();
                TextUpdated?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_SELENDOK:
                OnSelectionCommitted();
                SelectionCommitted?.Invoke(this, EventArgs.Empty);
                break;
            case Interop.CBN_SELENDCANCEL:
                OnSelectionCanceled();
                SelectionCanceled?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    /// <summary>
    ///  Called when the selection in the ComboBox is changed by the user.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Override this method to handle selection change logic in derived classes.
    ///  </para>
    /// </remarks>
    public virtual void OnSelectionChange()
    {
    }

    /// <summary>
    ///  Called when the ComboBox receives the keyboard focus.
    /// </summary>
    public virtual void OnGotFocus()
    {
    }

    /// <summary>
    ///  Called when the ComboBox loses the keyboard focus.
    /// </summary>
    public virtual void OnLostFocus()
    {
    }

    /// <summary>
    ///  Called when the ComboBox's drop-down list is about to be displayed.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    public virtual void OnDropDown()
    {
    }

    /// <summary>
    ///  Called when the ComboBox's drop-down list is closed.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    public virtual void OnCloseUp()
    {
    }

    /// <summary>
    ///  Called when the user double-clicks an item in the ComboBox's list.
    /// </summary>
    public virtual void OnDoubleClicked()
    {
    }

    /// <summary>
    ///  Called after the text in the ComboBox's edit portion has changed.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.DropDownList"/>.
    /// </remarks>
    public virtual void OnTextChanged()
    {
    }

    /// <summary>
    ///  Called when the text in the ComboBox's edit portion is about to update.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.DropDownList"/>.
    /// </remarks>
    public virtual void OnTextUpdated()
    {
    }

    /// <summary>
    ///  Called when the current selection is finalized by the user.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    public virtual void OnSelectionCommitted()
    {
    }

    /// <summary>
    ///  Called when the user cancels selection in the drop-down list.
    /// </summary>
    /// <remarks>
    ///  This is not sent for ComboBoxes created with <see cref="Styles.Simple"/>.
    /// </remarks>
    public virtual void OnSelectionCanceled()
    {
    }
}