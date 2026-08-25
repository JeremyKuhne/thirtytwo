// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

internal sealed class TextDragSource : DragSource
{
    private readonly Window _window;
    private readonly Func<string> _getText;
    private readonly Func<long> _getTextGeneration;
    private readonly Func<(int Start, int End)>? _getSelection;
    private readonly Action<int, int>? _setSelection;
    private readonly Action<string>? _replaceSelection;
    private readonly Func<Point, int>? _getCharacterIndex;
    private readonly Func<Point, bool>? _canStartDrag;
    private readonly bool _handlesWindowMessages;
    private Point _startPosition;
    private string? _pendingText;
    private (int Start, int End) _pendingSelection;
    private bool _hasCapture;

    internal TextDragSource(
        Window window,
        Func<string> getText,
        Func<long> getTextGeneration,
        Func<(int Start, int End)>? getSelection,
        Action<int, int>? setSelection,
        Action<string>? replaceSelection,
        Func<Point, int>? getCharacterIndex,
        Func<Point, bool>? canStartDrag,
        bool handleWindowMessages)
        : base(Validate(window, getText))
    {
        ArgumentNullException.ThrowIfNull(getTextGeneration);
        _window = window;
        _getText = getText;
        _getTextGeneration = getTextGeneration;
        _getSelection = getSelection;
        _setSelection = setSelection;
        _replaceSelection = replaceSelection;
        _getCharacterIndex = getCharacterIndex;
        _canStartDrag = canStartDrag;
        _handlesWindowMessages = handleWindowMessages;
        if (handleWindowMessages)
        {
            window.MessageHandler += WindowMessageHandler;
        }
    }

    internal string GetText() => _getText();

    protected override void OnDetaching()
    {
        if (_handlesWindowMessages)
        {
            _window.MessageHandler -= WindowMessageHandler;
        }

        ReleaseCapture();
        _pendingText = null;
    }

    private LRESULT? WindowMessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.LeftButtonDown:
                _startPosition = GetPosition(lParam);
                _pendingText = (_canStartDrag?.Invoke(_startPosition) ?? true) ? GetText() : null;
                if (_pendingText is { Length: > 0 } && _getSelection is not null)
                {
                    _pendingSelection = _getSelection();
                    _ = PInvoke.SetCapture(_window.Handle);
                    _hasCapture = true;
                    return (LRESULT)0;
                }

                break;
            case MessageType.MouseMove:
                if (((MouseKey)(uint)wParam & MouseKey.LeftButton) == 0)
                {
                    _pendingText = null;
                    ReleaseCapture();
                    break;
                }

                if (_pendingText is { Length: > 0 } text && HasMoved(_startPosition, GetPosition(lParam)))
                {
                    _pendingText = null;
                    ReleaseCapture();
                    if (_getSelection is null || _setSelection is null || _replaceSelection is null)
                    {
                        return (LRESULT)0;
                    }

                    _window.BeginTextDragSession(
                        _pendingSelection.Start,
                        _pendingSelection.End,
                        _getTextGeneration,
                        _getSelection,
                        _setSelection,
                        _replaceSelection);
                    DragDropEffects effect = DragDropEffects.None;
                    try
                    {
                        effect = StartDrag(text);
                    }
                    finally
                    {
                        _window.EndTextDragSession(effect);
                    }

                    return (LRESULT)0;
                }

                break;
            case MessageType.LeftButtonUp:
                if (_pendingText is not null && _setSelection is not null && _getCharacterIndex is not null)
                {
                    int index = _getCharacterIndex(GetPosition(lParam));
                    _pendingText = null;
                    ReleaseCapture();
                    _setSelection(index, index);
                    return (LRESULT)0;
                }

                _pendingText = null;
                break;
            case MessageType.CaptureChanged:
                _hasCapture = false;
                _pendingText = null;
                break;
            case MessageType.SetCursor:
                if (_canStartDrag is not null
                    && PInvoke.GetCursorPos(out Point cursorPosition)
                    && _window.ScreenToClient(ref cursorPosition)
                    && _canStartDrag(cursorPosition))
                {
                    _ = CursorId.Arrow.SetCursor();
                    return (LRESULT)1;
                }

                break;
        }

        return null;
    }

    private static Point GetPosition(LPARAM parameter)
        => new(unchecked((short)parameter.LOWORD), unchecked((short)parameter.HIWORD));

    private static bool HasMoved(Point start, Point current)
        => HasExceededDragThreshold(
            start,
            current,
            PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXDRAG),
            PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYDRAG));

    private static bool HasExceededDragThreshold(
        Point start,
        Point current,
        int horizontalThreshold,
        int verticalThreshold)
        => Math.Abs((long)current.X - start.X) > Math.Abs((long)horizontalThreshold)
            || Math.Abs((long)current.Y - start.Y) > Math.Abs((long)verticalThreshold);

    private void ReleaseCapture()
    {
        if (_hasCapture)
        {
            _hasCapture = false;
            _ = PInvoke.ReleaseCapture();
        }
    }

    private static Window Validate(Window window, Func<string> getText)
    {
        ArgumentNullException.ThrowIfNull(getText);
        return window;
    }
}
