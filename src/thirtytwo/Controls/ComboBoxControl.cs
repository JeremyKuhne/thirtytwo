// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

/// <summary>
///  Win32 ComboBox control wrapper.
/// </summary>
public unsafe partial class ComboBoxControl : RegisteredControl
{
    private static readonly WindowClass s_comboBoxClass = new("ComboBox");

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
}