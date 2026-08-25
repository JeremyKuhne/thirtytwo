// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Touki.TestSupport;
using Windows.ApplicationModel.DataTransfer;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.WinUI;
using NativeWindow = Windows.Window;
using XamlDragEventArgs = Microsoft.UI.Xaml.DragEventArgs;
using XamlDragEventHandler = Microsoft.UI.Xaml.DragEventHandler;

namespace IntegrationHost;

internal sealed class NativeTextDragScenario : IDisposable
{
    private static readonly TimeSpan s_inputPhaseTimeout = TimeSpan.FromSeconds(3);

    private const string SourcePrefix = "Before ";
    private const string SourceSuffix = " after";
    private const string TextBoxPayload = "TextBox payload";
    private const string RichEditBoxPayload = "RichEditBox payload";

    private readonly NativeWindow _parent;
    private readonly ScenarioReporter _reporter;
    private readonly TextDragSourceEditControl _source;
    private readonly WinUITextBox _textBoxTarget;
    private readonly WinUIRichEditBox _richEditBoxTarget;
    private readonly FrameworkElement[] _targetEditors;
    private readonly XamlDragEventHandler _targetDragEnterHandler;
    private readonly XamlDragEventHandler _targetDropHandler;
    private readonly ManualResetEventSlim _sourcePositionObserved = new();
    private readonly ManualResetEventSlim _sourceLeftButtonDownObserved = new();
    private readonly ManualResetEventSlim _sourceDragMoveObserved = new();
    private readonly ManualResetEventSlim _targetDragEnterObserved = new();
    private Action? _completion;
    private WinUITextControl? _activeTarget;
    private string? _activePayload;
    private string? _expectedSourceText;
    private Exception? _failure;
    private Point _originalCursorPosition;
    private Point _expectedSourceClientPoint;
    private DataPackageOperation _targetAcceptedOperation;
    private Exception? _targetDragEnterObservationFailure;
    private int _leftButtonDown;
    private int _loadedTargetCount;
    private int _dragIndex;
    private bool _started;
    private bool _beginQueued;
    private bool _inputScheduled;
    private bool _targetCommitted;
    private bool _targetDragEnterObservationQueued;
    private bool _sourceDeleted;
    private bool _verificationQueued;
    private bool _cursorPositionSaved;
    private bool _cursorPositionRestored;
    private bool _shutdownRequested;
    private bool _completed;
    private bool _disposed;

    internal NativeTextDragScenario(NativeWindow parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;

        TextDragSourceEditControl source = new(
            new Rectangle(20, 40, 330, 100),
            parent);
        WinUITextBox? textBoxTarget = null;
        WinUIRichEditBox? richEditBoxTarget = null;
        try
        {
            source.MessageHandler += SourceWindowMessage;
            source.EnableDrag = true;
            textBoxTarget = new(new Rectangle(420, 40, 330, 120), parent)
            {
                EnableDrop = true
            };
            richEditBoxTarget = new(new Rectangle(420, 200, 330, 150), parent)
            {
                EnableDrop = true
            };
        }
        catch
        {
            richEditBoxTarget?.Dispose();
            textBoxTarget?.Dispose();
            source.Dispose();
            throw;
        }

        _source = source;
        _textBoxTarget = textBoxTarget;
        _richEditBoxTarget = richEditBoxTarget;
        _targetEditors =
        [
            (FrameworkElement)(_textBoxTarget.Content
                ?? throw new InvalidOperationException("The WinUI TextBox editor was not created.")),
            (FrameworkElement)(_richEditBoxTarget.Content
                ?? throw new InvalidOperationException("The WinUI RichEditBox editor was not created."))
        ];
        _targetDragEnterHandler = TargetDragEnter;
        _targetDropHandler = TargetDrop;

        _source.SourceTextChanged += SourceTextChanged;
        _textBoxTarget.TextChanged += TextBoxTargetTextChanged;
        _richEditBoxTarget.TextChanged += RichEditBoxTargetTextChanged;
        foreach (FrameworkElement editor in _targetEditors)
        {
            if (editor.IsLoaded)
            {
                _loadedTargetCount++;
            }
            else
            {
                editor.Loaded += TargetLoaded;
            }

            editor.AddHandler(UIElement.DragEnterEvent, _targetDragEnterHandler, handledEventsToo: true);
            editor.AddHandler(UIElement.DropEvent, _targetDropHandler, handledEventsToo: true);
        }
    }

    internal void Start(Action completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        Ensure(!_started, "The native text-drag scenario was started more than once.");
        _started = true;
        _completion = completion;

        _parent.SetWindowPosition(
            WindowZOrder.TopMost,
            default,
            WindowPositionFlags.NoMove
                | WindowPositionFlags.NoSize
                | WindowPositionFlags.NoActivate
                | WindowPositionFlags.ShowWindow);
        _parent.UpdateWindow();
        _ = PInvoke.SetActiveWindow(_parent.Handle);
        _reporter.Write("native-text-drag-controls-created", _source.Handle);
        TryQueueNextDrag();
    }

    internal void VerifyAfterRun()
    {
        if (_failure is not null)
        {
            throw new InvalidOperationException("The native-to-WinUI text drag scenario failed.", _failure);
        }

        Ensure(_completed, "The native-to-WinUI text drag scenario did not complete both moves.");
        Ensure(_cursorPositionRestored, "The native-to-WinUI text drag scenario did not restore the cursor.");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (FrameworkElement editor in _targetEditors)
        {
            editor.Loaded -= TargetLoaded;
            editor.RemoveHandler(UIElement.DragEnterEvent, _targetDragEnterHandler);
            editor.RemoveHandler(UIElement.DropEvent, _targetDropHandler);
        }

        _richEditBoxTarget.TextChanged -= RichEditBoxTargetTextChanged;
        _textBoxTarget.TextChanged -= TextBoxTargetTextChanged;
        _source.SourceTextChanged -= SourceTextChanged;
        _source.MessageHandler -= SourceWindowMessage;
        if (!_completed)
        {
            TryReleaseLeftButton();
            TryRestoreCursorPosition();
        }

        _richEditBoxTarget.Dispose();
        _textBoxTarget.Dispose();
        _source.Dispose();
        _targetDragEnterObserved.Dispose();
        _sourceDragMoveObserved.Dispose();
        _sourceLeftButtonDownObserved.Dispose();
        _sourcePositionObserved.Dispose();
    }

    private void TargetLoaded(object sender, RoutedEventArgs eventArgs)
    {
        ((FrameworkElement)sender).Loaded -= TargetLoaded;
        _loadedTargetCount++;
        TryQueueNextDrag();
    }

    private void TryQueueNextDrag()
    {
        if (!_started
            || _loadedTargetCount != _targetEditors.Length
            || _beginQueued
            || _inputScheduled
            || _completed
            || _failure is not null
            || _disposed)
        {
            return;
        }

        _beginQueued = true;
        if (!_parent.Dispatcher.TryPost(BeginNextDrag))
        {
            Fail(new InvalidOperationException("Failed to queue the next native text drag."));
        }
    }

    private void BeginNextDrag()
    {
        _beginQueued = false;
        try
        {
            if (!_cursorPositionSaved)
            {
                if (!PInvoke.GetCursorPos(out _originalCursorPosition))
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }

                _cursorPositionSaved = true;
                _reporter.Write("native-text-drag-targets-loaded", _textBoxTarget.Handle);
            }

            WinUITextControl target = _dragIndex == 0 ? _textBoxTarget : _richEditBoxTarget;
            string payload = _dragIndex == 0 ? TextBoxPayload : RichEditBoxPayload;
            string targetName = GetActiveTargetName();
            target.Text = string.Empty;
            target.Select(0, 0);

            _source.Text = $"{SourcePrefix}{payload}{SourceSuffix}";
            int selectionStart = SourcePrefix.Length;
            int selectionEnd = checked(selectionStart + payload.Length);
            _source.SetSelection(selectionStart, selectionEnd);
            _source.SetFocus();
            Ensure(PInvoke.GetFocus() == _source.Handle, "The native drag source did not receive focus.");
            Ensure(
                _source.GetSelection() == (selectionStart, selectionEnd),
                "The native drag source did not retain the selected payload.");

            (Point sourceClientPoint, Point sourcePoint) = GetSourcePoints(selectionStart, selectionEnd);
            (Point dragStart, Point targetPoint) = GetTargetPoints(target);
            EnsureDragThresholdExceeded(sourcePoint, dragStart);

            _activeTarget = target;
            _activePayload = payload;
            _expectedSourceText = $"{SourcePrefix}{SourceSuffix}";
            _targetCommitted = false;
            _sourceDeleted = false;
            _verificationQueued = false;
            _expectedSourceClientPoint = sourceClientPoint;
            _sourcePositionObserved.Reset();
            _sourceLeftButtonDownObserved.Reset();
            _sourceDragMoveObserved.Reset();
            _targetDragEnterObserved.Reset();
            _targetAcceptedOperation = DataPackageOperation.None;
            _targetDragEnterObservationFailure = null;
            _targetDragEnterObservationQueued = false;
            _inputScheduled = true;
            _reporter.Write($"native-text-drag-{targetName}-source-ready", _source.Handle);

            Point sourceApproach = new(sourcePoint.X + 24, sourcePoint.Y + 16);
            _ = Task.Run(() => InjectDrag(sourceApproach, sourcePoint, dragStart, targetPoint, targetName, target.Handle));
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    private (Point Client, Point Screen) GetSourcePoints(int selectionStart, int selectionEnd)
    {
        int characterIndex = selectionStart + ((selectionEnd - selectionStart) / 2);
        EditBase source = _source;
        Point clientPoint = source.TestAccessor.Dynamic.GetCharacterPosition(characterIndex);
        clientPoint.Offset(1, 1);
        int hitIndex = source.TestAccessor.Dynamic.GetCharacterIndexFromPoint(clientPoint);
        Ensure(
            hitIndex >= selectionStart && hitIndex < selectionEnd,
            $"The native source point resolved to character {hitIndex}, outside the selected range.");
        Point screenPoint = clientPoint;
        if (!_source.ClientToScreen(ref screenPoint))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return (clientPoint, screenPoint);
    }

    private static (Point DragStart, Point Target) GetTargetPoints(WinUITextControl target)
    {
        Rectangle client = target.GetClientRectangle();
        Ensure(client.Width >= 20 && client.Height >= 20, "The WinUI drop target has no usable client area.");
        Point targetPoint = new(client.Width / 2, client.Height / 2);
        if (!target.ClientToScreen(ref targetPoint))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return (new Point(targetPoint.X - 8, targetPoint.Y), targetPoint);
    }

    private static void EnsureDragThresholdExceeded(Point source, Point dragStart)
    {
        long horizontalDistance = Math.Abs((long)dragStart.X - source.X);
        long verticalDistance = Math.Abs((long)dragStart.Y - source.Y);
        long horizontalThreshold = Math.Abs((long)PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXDRAG));
        long verticalThreshold = Math.Abs((long)PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYDRAG));
        Ensure(
            horizontalDistance > horizontalThreshold || verticalDistance > verticalThreshold,
            "The source and target positions do not exceed the configured system drag threshold.");
    }

    private void InjectDrag(
        Point sourceApproach,
        Point source,
        Point dragStart,
        Point target,
        string targetName,
        HWND targetHandle)
    {
        try
        {
            MouseInput.Move(sourceApproach);
            MouseInput.Move(source);
            WaitForInputPhase(_sourcePositionObserved, "the cursor to reach the native source");

            MouseInput.PressLeftButton();
            Volatile.Write(ref _leftButtonDown, 1);
            WaitForInputPhase(_sourceLeftButtonDownObserved, "the native source to receive left-button down");

            MouseInput.Move(dragStart);
            WaitForInputPhase(_sourceDragMoveObserved, "the native source to receive held-button movement");

            MouseInput.Move(target);
            WaitForInputPhase(_targetDragEnterObserved, "the WinUI target to receive routed DragEnter");
            if (_targetDragEnterObservationFailure is { } observationFailure)
            {
                throw new InvalidOperationException(
                    "Failed to observe the WinUI target after routed DragEnter dispatch.",
                    observationFailure);
            }

            Ensure(
                _targetAcceptedOperation == DataPackageOperation.Move,
                $"The WinUI target accepted {_targetAcceptedOperation} instead of Move.");

            _reporter.Write($"native-text-drag-{targetName}-mouse-left-up-injecting", targetHandle);
            ReleaseLeftButton();
            _reporter.Write($"native-text-drag-{targetName}-mouse-left-up-injected", targetHandle);
        }
        catch (Exception exception)
        {
            try
            {
                ReleaseLeftButton();
            }
            catch (Exception cleanupException)
            {
                exception = new AggregateException(exception, cleanupException);
            }

            if (!_parent.Dispatcher.TryPost(() => Fail(exception)))
            {
                _ = Interlocked.CompareExchange(
                    ref _failure,
                    new InvalidOperationException("Failed to report mouse-input failure on the owner thread.", exception),
                    null);
                try
                {
                    _parent.PostMessage(MessageType.Close);
                }
                catch
                {
                }
            }
        }
    }

    private LRESULT? SourceWindowMessage(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        if (!_inputScheduled)
        {
            return null;
        }

        string targetName = GetActiveTargetName();
        switch (message)
        {
            case MessageType.MouseMove:
                Point position = new(unchecked((short)lParam.LOWORD), unchecked((short)lParam.HIWORD));
                if (((MouseKey)(uint)wParam & MouseKey.LeftButton) == 0)
                {
                    if (!_sourcePositionObserved.IsSet
                        && Math.Abs(position.X - _expectedSourceClientPoint.X) <= 2
                        && Math.Abs(position.Y - _expectedSourceClientPoint.Y) <= 2)
                    {
                        _reporter.Write($"native-text-drag-{targetName}-source-positioned", _source.Handle);
                        _sourcePositionObserved.Set();
                    }
                }
                else if (!_sourceDragMoveObserved.IsSet)
                {
                    _reporter.Write($"native-text-drag-{targetName}-source-drag-move", _source.Handle);
                    _sourceDragMoveObserved.Set();
                }

                break;
            case MessageType.LeftButtonDown when !_sourceLeftButtonDownObserved.IsSet:
                _reporter.Write($"native-text-drag-{targetName}-source-left-down", _source.Handle);
                _sourceLeftButtonDownObserved.Set();
                break;
        }

        return null;
    }

    private void TargetDragEnter(object sender, XamlDragEventArgs eventArgs)
    {
        if (!_inputScheduled
            || _targetDragEnterObserved.IsSet
            || _targetDragEnterObservationQueued
            || !ReferenceEquals(sender, _activeTarget?.Content))
        {
            return;
        }

        _targetDragEnterObservationQueued = true;
        WinUITextControl target = _activeTarget;
        string targetName = GetActiveTargetName();
        if (!_parent.Dispatcher.TryPost(() => CompleteTargetDragEnterObservation(target, targetName, eventArgs)))
        {
            _targetDragEnterObservationFailure = new InvalidOperationException(
                "Failed to queue the post-dispatch WinUI DragEnter observation.");
            _targetDragEnterObserved.Set();
        }
    }

    private void CompleteTargetDragEnterObservation(
        WinUITextControl target,
        string targetName,
        XamlDragEventArgs eventArgs)
    {
        try
        {
            Ensure(_inputScheduled, "The WinUI DragEnter observation ran after input completed.");
            Ensure(ReferenceEquals(target, _activeTarget), "The active WinUI target changed during DragEnter dispatch.");
            bool containsText = eventArgs.DataView.Contains(StandardDataFormats.Text);
            _targetAcceptedOperation = eventArgs.AcceptedOperation;
            _reporter.Write(
                $"native-text-drag-{targetName}-target-drag-enter",
                target.Handle,
                $"ContainsText={containsText}; Allowed={eventArgs.AllowedOperations}; Modifiers={eventArgs.Modifiers}; Accepted={_targetAcceptedOperation}");
        }
        catch (Exception exception)
        {
            _targetDragEnterObservationFailure = exception;
        }
        finally
        {
            _targetDragEnterObserved.Set();
        }
    }

    private void TargetDrop(object sender, XamlDragEventArgs eventArgs)
    {
        if (!_inputScheduled || !ReferenceEquals(sender, _activeTarget?.Content))
        {
            return;
        }

        _reporter.Write(
            $"native-text-drag-{GetActiveTargetName()}-target-drop",
            _activeTarget.Handle,
            $"Allowed={eventArgs.AllowedOperations}; Modifiers={eventArgs.Modifiers}; Accepted={eventArgs.AcceptedOperation}");
    }

    private static void WaitForInputPhase(ManualResetEventSlim signal, string description)
    {
        if (!signal.Wait(s_inputPhaseTimeout))
        {
            throw new TimeoutException($"Timed out waiting for {description}.");
        }
    }

    private void TextBoxTargetTextChanged(object? sender, EventArgs eventArgs)
        => TargetTextChanged(_textBoxTarget);

    private void RichEditBoxTargetTextChanged(object? sender, EventArgs eventArgs)
        => TargetTextChanged(_richEditBoxTarget);

    private void TargetTextChanged(WinUITextControl target)
    {
        if (!_inputScheduled
            || _targetCommitted
            || !ReferenceEquals(target, _activeTarget)
            || target.Text != _activePayload)
        {
            return;
        }

        _targetCommitted = true;
        _reporter.Write($"native-text-drag-{GetActiveTargetName()}-target-committed", target.Handle);
        TryQueueVerification();
    }

    private void SourceTextChanged(object? sender, EventArgs eventArgs)
    {
        if (!_inputScheduled || _sourceDeleted || _source.Text != _expectedSourceText)
        {
            return;
        }

        _sourceDeleted = true;
        _reporter.Write($"native-text-drag-{GetActiveTargetName()}-source-deleted", _source.Handle);
        TryQueueVerification();
    }

    private void TryQueueVerification()
    {
        if (!_targetCommitted || !_sourceDeleted || _verificationQueued)
        {
            return;
        }

        _verificationQueued = true;
        if (!_parent.Dispatcher.TryPost(VerifyActiveDrag))
        {
            Fail(new InvalidOperationException("Failed to queue native text-drag verification."));
        }
    }

    private void VerifyActiveDrag()
    {
        try
        {
            WinUITextControl target = _activeTarget
                ?? throw new InvalidOperationException("No WinUI drop target was active.");
            string payload = _activePayload
                ?? throw new InvalidOperationException("No native drag payload was active.");
            Ensure(target.Text == payload, "The WinUI target did not retain the complete native payload.");
            Ensure(
                target.SelectionStart == 0 && target.SelectionLength == payload.Length,
                "The WinUI target did not select the inserted native payload.");
            Ensure(_source.Text == _expectedSourceText, "The native source did not delete the moved selection.");
            Ensure(
                _source.GetSelection() == (SourcePrefix.Length, SourcePrefix.Length),
                "The native source selection did not collapse at the deleted range.");

            string targetName = GetActiveTargetName();
            _reporter.Write($"native-text-drag-{targetName}-move-verified", target.Handle);
            _dragIndex++;
            _inputScheduled = false;
            _activeTarget = null;
            _activePayload = null;
            _expectedSourceText = null;
            if (_dragIndex == 2)
            {
                Complete();
            }
            else
            {
                TryQueueNextDrag();
            }
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    private void Complete()
    {
        TryRestoreCursorPosition();
        if (_failure is not null)
        {
            RequestShutdown();
            return;
        }

        _completed = true;
        _reporter.Write("native-text-drag-completed", _parent.Handle);
        RequestShutdown();
    }

    private void Fail(Exception exception)
    {
        if (Interlocked.CompareExchange(ref _failure, exception, null) is not null)
        {
            return;
        }

        TryReleaseLeftButton();
        TryRestoreCursorPosition();
        _reporter.Write("native-text-drag-failed", message: _failure.ToString());
        RequestShutdown();
    }

    private void TryReleaseLeftButton()
    {
        try
        {
            ReleaseLeftButton();
        }
        catch (Exception exception)
        {
            _ = Interlocked.CompareExchange(ref _failure, exception, null);
        }
    }

    private void ReleaseLeftButton()
    {
        if (Interlocked.Exchange(ref _leftButtonDown, 0) == 0)
        {
            return;
        }

        try
        {
            MouseInput.ReleaseLeftButton();
        }
        catch
        {
            Volatile.Write(ref _leftButtonDown, 1);
            throw;
        }
    }

    private void TryRestoreCursorPosition()
    {
        if (!_cursorPositionSaved || _cursorPositionRestored)
        {
            return;
        }

        if (!PInvoke.SetCursorPos(_originalCursorPosition.X, _originalCursorPosition.Y))
        {
            _ = Interlocked.CompareExchange(
                ref _failure,
                new Win32Exception(Marshal.GetLastPInvokeError(), "Failed to restore the cursor position."),
                null);
            return;
        }

        _cursorPositionRestored = true;
    }

    private void RequestShutdown()
    {
        if (_shutdownRequested)
        {
            return;
        }

        _shutdownRequested = true;
        _completion?.Invoke();
    }

    private string GetActiveTargetName() => _dragIndex == 0 ? "textbox" : "rich-edit-box";

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}