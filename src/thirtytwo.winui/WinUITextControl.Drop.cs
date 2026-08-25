// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using global::Windows.ApplicationModel.DataTransfer;
using global::Windows.Foundation;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Windows.Win32;
using Windows.Win32.Foundation;
using DataTransferDragDropModifiers = global::Windows.ApplicationModel.DataTransfer.DragDrop.DragDropModifiers;
using XamlControl = Microsoft.UI.Xaml.Controls.Control;
using XamlDragEventArgs = Microsoft.UI.Xaml.DragEventArgs;
using XamlRichEditBox = Microsoft.UI.Xaml.Controls.RichEditBox;
using XamlTextBox = Microsoft.UI.Xaml.Controls.TextBox;

namespace Windows.WinUI;

public abstract partial class WinUITextControl
{
    private bool _textDropRequested;
    private HWND _focusBeforeTextDrop;
    private CompositionColorBrush? _textDropCaretBrush;
    private SpriteVisual? _textDropCaretVisual;
    private bool _textDropCaretTracking;
    private bool _textDropCaretVisible;
    private DragEventHandler? _editorDragEnterHandler;
    private DragEventHandler? _editorDragLeaveHandler;
    private DragEventHandler? _editorDragOverHandler;
    private DragEventHandler? _editorDropHandler;

    private void SetXamlTextDropEnabled(bool value)
    {
        if (value == _textDropRequested)
        {
            return;
        }

        _textDropRequested = value;
        XamlControl editor = GetEditor();
        if (!value)
        {
            editor.AllowDrop = false;
            if (_editorDragEnterHandler is not null)
            {
                editor.RemoveHandler(UIElement.DragEnterEvent, _editorDragEnterHandler);
                _editorDragEnterHandler = null;
            }

            if (_editorDragOverHandler is not null)
            {
                editor.RemoveHandler(UIElement.DragOverEvent, _editorDragOverHandler);
                _editorDragOverHandler = null;
            }

            if (_editorDragLeaveHandler is not null)
            {
                editor.RemoveHandler(UIElement.DragLeaveEvent, _editorDragLeaveHandler);
                _editorDragLeaveHandler = null;
            }

            if (_editorDropHandler is not null)
            {
                editor.RemoveHandler(UIElement.DropEvent, _editorDropHandler);
                _editorDropHandler = null;
            }

            CancelTextDropInsertion();
            return;
        }

        try
        {
            editor.AllowDrop = true;
            _editorDragEnterHandler = EditorDragEnter;
            _editorDragOverHandler = EditorDragOver;
            _editorDragLeaveHandler = EditorDragLeave;
            _editorDropHandler = EditorDrop;
            editor.AddHandler(UIElement.DragEnterEvent, _editorDragEnterHandler, handledEventsToo: true);
            editor.AddHandler(UIElement.DragOverEvent, _editorDragOverHandler, handledEventsToo: true);
            editor.AddHandler(UIElement.DragLeaveEvent, _editorDragLeaveHandler, handledEventsToo: true);
            editor.AddHandler(UIElement.DropEvent, _editorDropHandler, handledEventsToo: true);
        }
        catch
        {
            SetXamlTextDropEnabled(value: false);
            throw;
        }
    }

    private protected override void OnXamlSourceChanging()
    {
        if (_textDropRequested)
        {
            CancelTextDropInsertion();
        }

        base.OnXamlSourceChanging();
    }

    private void EditorDragEnter(object sender, XamlDragEventArgs eventArgs)
        => UpdateRoutedTextDrop(eventArgs, "TextDragEnter");

    private void EditorDragOver(object sender, XamlDragEventArgs eventArgs)
        => UpdateRoutedTextDrop(eventArgs, "TextDragOver");

    private void EditorDragLeave(object sender, XamlDragEventArgs eventArgs)
    {
        try
        {
            eventArgs.Handled = true;
            CancelTextDropInsertion();
        }
        catch (Exception exception)
        {
            ReportNativeCallbackFailure("TextDragLeave", exception);
        }
    }

    private async void EditorDrop(object sender, XamlDragEventArgs eventArgs)
    {
        DragOperationDeferral? deferral = null;
        try
        {
            eventArgs.Handled = true;
            deferral = eventArgs.GetDeferral();
            if (!TryGetRoutedTextDrop(eventArgs, out DataPackageOperation effect, out int insertionStart))
            {
                CancelTextDropInsertion();
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            string text = await eventArgs.DataView.GetTextAsync();
            if (text.Length == 0 || text.Length > DropDataObject.DefaultMaximumTextLength)
            {
                CancelTextDropInsertion();
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            XamlControl editor = GetEditor();
            if ((uint)insertionStart > (uint)GetTextLength(editor))
            {
                CancelTextDropInsertion();
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            int insertionEnd = checked(insertionStart + text.Length);
            Select(insertionStart, 0);
            // GetTextAsync yielded; selection callbacks may also mutate the document synchronously.
            if (SelectionStart != insertionStart
                || SelectionLength != 0
                || insertionStart > GetTextLength(editor))
            {
                CancelTextDropInsertion();
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            SelectedText = text;
            Select(insertionStart, insertionEnd - insertionStart);
            CommitTextDropInsertion();
            eventArgs.AcceptedOperation = effect;
        }
        catch (Exception exception)
        {
            try
            {
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                CancelTextDropInsertion();
            }
            catch (Exception cleanupException)
            {
                ReportNativeCallbackFailure("TextDropCleanup", cleanupException);
            }

            ReportNativeCallbackFailure("TextDrop", exception);
        }
        finally
        {
            try
            {
                deferral?.Complete();
            }
            catch (Exception exception)
            {
                ReportNativeCallbackFailure("TextDropDeferral", exception);
            }
        }
    }

    private void UpdateRoutedTextDrop(XamlDragEventArgs eventArgs, string operation)
    {
        try
        {
            eventArgs.Handled = true;
            if (!TryGetRoutedTextDrop(eventArgs, out DataPackageOperation effect, out int insertionStart))
            {
                CancelTextDropInsertion();
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            if (!_textDropCaretTracking)
            {
                BeginTextDropInsertion();
            }

            UpdateTextDropCaret(insertionStart);
            eventArgs.AcceptedOperation = effect;
        }
        catch (Exception exception)
        {
            try
            {
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                CancelTextDropInsertion();
            }
            catch (Exception cleanupException)
            {
                ReportNativeCallbackFailure($"{operation}Cleanup", cleanupException);
            }

            ReportNativeCallbackFailure(operation, exception);
        }
    }

    private bool TryGetRoutedTextDrop(
        XamlDragEventArgs eventArgs,
        out DataPackageOperation effect,
        out int insertionStart)
    {
        effect = GetRoutedTextDropEffect(eventArgs);
        if (effect == DataPackageOperation.None)
        {
            insertionStart = -1;
            return false;
        }

        XamlControl editor = GetEditor();
        insertionStart = GetTextIndexFromPoint(editor, eventArgs.GetPosition(editor));
        return true;
    }

    private DataPackageOperation GetRoutedTextDropEffect(XamlDragEventArgs eventArgs)
    {
        if (!_textDropRequested || !eventArgs.DataView.Contains(StandardDataFormats.Text))
        {
            return DataPackageOperation.None;
        }

        if ((eventArgs.Modifiers & DataTransferDragDropModifiers.Control) != 0
            && (eventArgs.AllowedOperations & DataPackageOperation.Copy) != 0)
        {
            return DataPackageOperation.Copy;
        }

        if ((eventArgs.AllowedOperations & DataPackageOperation.Move) != 0)
        {
            return DataPackageOperation.Move;
        }

        return (eventArgs.AllowedOperations & DataPackageOperation.Copy) != 0
            ? DataPackageOperation.Copy
            : DataPackageOperation.None;
    }

    private void BeginTextDropInsertion()
    {
        _focusBeforeTextDrop = PInvoke.GetFocus();
        _textDropCaretTracking = true;
    }

    private void CancelTextDropInsertion()
    {
        EndTextDropCaret();
        if (!_focusBeforeTextDrop.IsNull)
        {
            _ = PInvoke.SetFocus(_focusBeforeTextDrop);
        }

        _focusBeforeTextDrop = default;
    }

    private void CommitTextDropInsertion()
    {
        EndTextDropCaret();
        _ = GetEditor().Focus(FocusState.Pointer);
        _focusBeforeTextDrop = default;
    }

    private void UpdateTextDropCaret(int index)
    {
        if (!_textDropCaretTracking)
        {
            return;
        }

        XamlControl editor = GetEditor();
        if (editor is XamlTextBox { Text.Length: 0 })
        {
            HideTextDropCaret(editor);
            return;
        }

        Rect caret = _richEditBox is not null
            ? GetRichEditCaret(_richEditBox, index)
            : GetTextBoxCaret(_textBox!, index);
        if (!_textDropCaretVisible)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(editor).Compositor;
            global::System.Drawing.Color color = ForegroundColor;
            _textDropCaretBrush = compositor.CreateColorBrush(
                global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
            _textDropCaretVisual = compositor.CreateSpriteVisual();
            _textDropCaretVisual.Brush = _textDropCaretBrush;
            _textDropCaretVisual.IsHitTestVisible = false;
            _textDropCaretVisual.IsPixelSnappingEnabled = true;
            ElementCompositionPreview.SetElementChildVisual(editor, _textDropCaretVisual);
        }

        _textDropCaretVisual!.Offset = new Vector3((float)caret.X, (float)caret.Y, 0);
        _textDropCaretVisual.Size = new Vector2(1, Math.Max(1, (float)caret.Height));
        _textDropCaretVisible = true;
    }

    private void EndTextDropCaret()
    {
        HideTextDropCaret(GetEditor());
        _textDropCaretTracking = false;
    }

    private void HideTextDropCaret(XamlControl editor)
    {
        if (_textDropCaretVisual is not null)
        {
            ElementCompositionPreview.SetElementChildVisual(editor, null!);
            _textDropCaretVisual.Dispose();
            _textDropCaretVisual = null;
        }

        _textDropCaretBrush?.Dispose();
        _textDropCaretBrush = null;
        _textDropCaretVisible = false;
    }

    private int GetTextIndexFromPoint(XamlControl editor, Point position)
    {
        if (editor is XamlTextBox textBox)
        {
            return GetTextBoxIndexFromPoint(textBox, position);
        }

        XamlRichEditBox richEditBox = (XamlRichEditBox)editor;
        ITextRange range = richEditBox.Document.GetRange(0, 0);
        range.SetPoint(position, PointOptions.ClientCoordinates | PointOptions.AllowOffClient, extend: false);
        return Math.Clamp(range.StartPosition, 0, GetTextLength(editor));
    }

    private static int GetTextLength(XamlControl editor)
        => editor is XamlTextBox textBox
            ? textBox.Text.Length
            : Math.Max(0, ((XamlRichEditBox)editor).Document.Selection.StoryLength - 1);

    private static int GetTextBoxIndexFromPoint(XamlTextBox textBox, Point position)
    {
        int textLength = textBox.Text.Length;
        if (textLength == 0)
        {
            return 0;
        }

        int low = 0;
        int high = textLength;
        bool rightToLeft = textBox.FlowDirection == FlowDirection.RightToLeft;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            Rect caret = GetTextBoxCaret(textBox, middle);
            if (CaretPrecedesPoint(caret, position, rightToLeft))
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        int following = low;
        if (following == 0)
        {
            return 0;
        }

        if (following == textLength)
        {
            Rect finalCaret = GetTextBoxCaret(textBox, textLength);
            Rect previousCaret = GetTextBoxCaret(textBox, textLength - 1);
            return DistanceSquared(finalCaret, position) <= DistanceSquared(previousCaret, position)
                ? textLength
                : textLength - 1;
        }

        Rect before = GetTextBoxCaret(textBox, following - 1);
        Rect after = GetTextBoxCaret(textBox, following);
        return DistanceSquared(after, position) <= DistanceSquared(before, position)
            ? following
            : following - 1;
    }

    private static Rect GetTextBoxCaret(XamlTextBox textBox, int index)
        => index == textBox.Text.Length
            ? textBox.GetRectFromCharacterIndex(index - 1, trailingEdge: true)
            : textBox.GetRectFromCharacterIndex(index, trailingEdge: false);

    private static Rect GetRichEditCaret(XamlRichEditBox richEditBox, int index)
    {
        ITextRange range = richEditBox.Document.GetRange(index, index);
        PointOptions options = PointOptions.ClientCoordinates | PointOptions.AllowOffClient;
        range.GetPoint(
            HorizontalCharacterAlignment.Left,
            VerticalCharacterAlignment.Top,
            options,
            out Point topLeft);
        range.GetPoint(
            HorizontalCharacterAlignment.Right,
            VerticalCharacterAlignment.Bottom,
            options,
            out Point bottomRight);
        double left = Math.Min(topLeft.X, bottomRight.X);
        double top = Math.Min(topLeft.Y, bottomRight.Y);
        return new Rect(
            left,
            top,
            Math.Abs(bottomRight.X - topLeft.X),
            Math.Abs(bottomRight.Y - topLeft.Y));
    }

    private static bool CaretPrecedesPoint(Rect caret, Point point, bool rightToLeft)
    {
        if (caret.Bottom < point.Y)
        {
            return true;
        }

        if (caret.Top > point.Y)
        {
            return false;
        }

        return rightToLeft ? caret.X > point.X : caret.X < point.X;
    }

    private static double DistanceSquared(Rect caret, Point point)
    {
        double horizontal = caret.X - point.X;
        double vertical = point.Y < caret.Top
            ? caret.Top - point.Y
            : point.Y > caret.Bottom
                ? point.Y - caret.Bottom
                : 0;
        return (horizontal * horizontal) + (vertical * vertical);
    }
}