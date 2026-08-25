// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

internal sealed class TextDropTarget : DropTarget
{
    private readonly Func<(int Start, int End)>? _getSelection;
    private readonly Func<Point, int>? _getInsertionIndex;
    private readonly Func<int>? _getTextLength;
    private readonly Action<string> _replaceSelection;
    private readonly Action<int, int>? _setSelection;
    private readonly Action? _beginInsertion;
    private readonly Action? _cancelInsertion;
    private readonly Action? _commitInsertion;
    private readonly bool _focusForInsertion;
    private readonly Window _window;
    private HWND _originalFocus;
    private (int Start, int End) _originalSelection;
    private int _insertionIndex;
    private bool _trackingInsertion;

    internal TextDropTarget(
        Window window,
        Action<string> replaceSelection,
        Func<(int Start, int End)>? getSelection = null,
        Action<int, int>? setSelection = null,
        Func<Point, int>? getInsertionIndex = null,
        Func<int>? getTextLength = null,
        bool focusForInsertion = false,
        Action? beginInsertion = null,
        Action? cancelInsertion = null,
        Action? commitInsertion = null)
        : base(Validate(window, replaceSelection, getSelection, setSelection, getInsertionIndex, getTextLength))
    {
        _window = window;
        _replaceSelection = replaceSelection;
        _getSelection = getSelection;
        _setSelection = setSelection;
        _getInsertionIndex = getInsertionIndex;
        _getTextLength = getTextLength;
        _focusForInsertion = focusForInsertion;
        _beginInsertion = beginInsertion;
        _cancelInsertion = cancelInsertion;
        _commitInsertion = commitInsertion;
    }

    public override void OnDragEnter(DragEventArgs eventArgs)
    {
        UpdateInsertion(eventArgs);
        base.OnDragEnter(eventArgs);
    }

    public override void OnDragOver(DragEventArgs eventArgs)
    {
        UpdateInsertion(eventArgs);
        base.OnDragOver(eventArgs);
    }

    public override void OnDragLeave()
    {
        RestoreSelection();
        base.OnDragLeave();
    }

    public override void OnDragDrop(DragEventArgs eventArgs)
    {
        bool inserted = false;
        try
        {
            DragDropEffects selectedEffect = GetTextEffect(eventArgs);
            if (selectedEffect != DragDropEffects.None && eventArgs.Data.GetText() is { Length: > 0 } text)
            {
                TextDragSession? session = Window.ActiveTextDragSession;
                bool sameSource = session?.IsSource(_window) == true;
                int insertionStart = MoveInsertion(eventArgs.ScreenLocation, preserveSelection: sameSource);
                if (insertionStart < 0
                    || (selectedEffect == DragDropEffects.Move
                        && session?.ContainsSourceIndex(_window, insertionStart) == true))
                {
                    RestoreSelection();
                    base.OnDragDrop(eventArgs);
                    return;
                }

                if (_setSelection is not null)
                {
                    _setSelection(insertionStart, insertionStart);
                }

                _replaceSelection(text);
                if (_setSelection is not null)
                {
                    _setSelection(insertionStart, checked(insertionStart + text.Length));
                }

                session?.RecordSameSourceInsertion(_window, insertionStart, text.Length);
                eventArgs.Effect = selectedEffect;
                inserted = true;
            }
        }
        finally
        {
            if (inserted)
            {
                _trackingInsertion = false;
                _originalFocus = default;
                _commitInsertion?.Invoke();
            }
            else
            {
                RestoreSelection();
            }
        }

        base.OnDragDrop(eventArgs);
    }

    private protected override void OnDragOperationFailed() => RestoreSelection();

    private static Window Validate(
        Window window,
        Action<string> replaceSelection,
        Func<(int Start, int End)>? getSelection,
        Action<int, int>? setSelection,
        Func<Point, int>? getInsertionIndex,
        Func<int>? getTextLength)
    {
        ArgumentNullException.ThrowIfNull(replaceSelection);
        if (getInsertionIndex is not null
            && (getSelection is null || setSelection is null || getTextLength is null))
        {
            throw new ArgumentException(
                "Selection and text-length providers are required with an insertion-index provider.",
                nameof(getInsertionIndex));
        }

        return window;
    }

    private void UpdateInsertion(DragEventArgs eventArgs)
    {
        DragDropEffects selectedEffect = GetTextEffect(eventArgs);
        if (selectedEffect == DragDropEffects.None)
        {
            RestoreSelection();
            return;
        }

        TextDragSession? session = Window.ActiveTextDragSession;
        bool sameSource = session?.IsSource(_window) == true;
        int insertionIndex = MoveInsertion(eventArgs.ScreenLocation, preserveSelection: sameSource);
        if (insertionIndex < 0
            || (selectedEffect == DragDropEffects.Move
                && session?.ContainsSourceIndex(_window, insertionIndex) == true))
        {
            RestoreSelection();
            return;
        }

        eventArgs.Effect = selectedEffect;
    }

    private int MoveInsertion(Point screenLocation, bool preserveSelection)
    {
        if (_getSelection is null || _setSelection is null || _getInsertionIndex is null)
        {
            return _getSelection?.Invoke().Start ?? -1;
        }

        if (!_trackingInsertion)
        {
            _originalSelection = _getSelection();
            if (_focusForInsertion)
            {
                _originalFocus = PInvoke.GetFocus();
                _window.SetFocus();
            }

            _beginInsertion?.Invoke();
            _trackingInsertion = true;
        }

        _insertionIndex = _getInsertionIndex(screenLocation);
        if (_getTextLength is null || _insertionIndex < 0 || _insertionIndex > _getTextLength())
        {
            return -1;
        }

        if (!preserveSelection)
        {
            _setSelection(_insertionIndex, _insertionIndex);
        }

        return _insertionIndex;
    }

    private void RestoreSelection()
    {
        if (!_trackingInsertion)
        {
            return;
        }

        _trackingInsertion = false;
        _setSelection!(_originalSelection.Start, _originalSelection.End);
        if (_focusForInsertion)
        {
            _ = PInvoke.SetFocus(_originalFocus);
        }

        _cancelInsertion?.Invoke();
        _originalFocus = default;
    }

    private static DragDropEffects GetTextEffect(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.ContainsText)
        {
            return DragDropEffects.None;
        }

        if ((eventArgs.KeyState & DragDropKeyStates.ControlKey) != 0
            && (eventArgs.AllowedEffect & DragDropEffects.Copy) != 0)
        {
            return DragDropEffects.Copy;
        }

        if ((eventArgs.AllowedEffect & DragDropEffects.Move) != 0)
        {
            return DragDropEffects.Move;
        }

        return (eventArgs.AllowedEffect & DragDropEffects.Copy) != 0
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }
}
