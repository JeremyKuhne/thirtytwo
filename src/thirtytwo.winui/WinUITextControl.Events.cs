// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using XamlCandidateWindowBoundsChangedEventArgs = Microsoft.UI.Xaml.Controls.CandidateWindowBoundsChangedEventArgs;
using XamlContextMenuEventArgs = Microsoft.UI.Xaml.Controls.ContextMenuEventArgs;
using XamlRichEditBox = Microsoft.UI.Xaml.Controls.RichEditBox;
using XamlRichEditBoxSelectionChangingEventArgs = Microsoft.UI.Xaml.Controls.RichEditBoxSelectionChangingEventArgs;
using XamlRichEditBoxTextChangingEventArgs = Microsoft.UI.Xaml.Controls.RichEditBoxTextChangingEventArgs;
using XamlTextBox = Microsoft.UI.Xaml.Controls.TextBox;
using XamlTextBoxBeforeTextChangingEventArgs = Microsoft.UI.Xaml.Controls.TextBoxBeforeTextChangingEventArgs;
using XamlTextBoxSelectionChangingEventArgs = Microsoft.UI.Xaml.Controls.TextBoxSelectionChangingEventArgs;
using XamlTextBoxTextChangingEventArgs = Microsoft.UI.Xaml.Controls.TextBoxTextChangingEventArgs;
using XamlTextCompositionChangedEventArgs = Microsoft.UI.Xaml.Controls.TextCompositionChangedEventArgs;
using XamlTextCompositionEndedEventArgs = Microsoft.UI.Xaml.Controls.TextCompositionEndedEventArgs;
using XamlTextCompositionStartedEventArgs = Microsoft.UI.Xaml.Controls.TextCompositionStartedEventArgs;
using XamlTextControlCopyingToClipboardEventArgs = Microsoft.UI.Xaml.Controls.TextControlCopyingToClipboardEventArgs;
using XamlTextControlCuttingToClipboardEventArgs = Microsoft.UI.Xaml.Controls.TextControlCuttingToClipboardEventArgs;
using XamlTextControlPasteEventArgs = Microsoft.UI.Xaml.Controls.TextControlPasteEventArgs;

namespace Windows.WinUI;

public abstract partial class WinUITextControl
{
    /// <summary>
    ///  Occurs when IME candidate-window bounds change.
    /// </summary>
    public event EventHandler<WinUICandidateWindowBoundsChangedEventArgs>? CandidateWindowBoundsChanged;

    /// <summary>
    ///  Occurs when the editor requests its context menu.
    /// </summary>
    public event EventHandler<WinUITextContextMenuOpeningEventArgs>? ContextMenuOpening;

    /// <summary>
    ///  Occurs before selected content is copied to the clipboard.
    /// </summary>
    public event EventHandler<WinUITextClipboardEventArgs>? CopyingToClipboard;

    /// <summary>
    ///  Occurs before selected content is cut to the clipboard.
    /// </summary>
    public event EventHandler<WinUITextClipboardEventArgs>? CuttingToClipboard;

    /// <summary>
    ///  Occurs before clipboard content is pasted.
    /// </summary>
    public event EventHandler<WinUITextClipboardEventArgs>? Paste;

    /// <summary>
    ///  Occurs after the text selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Occurs before the text selection changes.
    /// </summary>
    /// <remarks>Handlers receive the proposed range and can cancel before the platform applies it.</remarks>
    public event EventHandler<WinUITextSelectionChangingEventArgs>? SelectionChanging;

    /// <summary>
    ///  Occurs after editor text changes.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    ///  Occurs synchronously while editor text is changing.
    /// </summary>
    /// <remarks>
    ///  This event mirrors WinUI's in-flight update notification and may be raised for content-changing and
    ///  non-content-changing transitions.
    /// </remarks>
    public event EventHandler<WinUITextChangingEventArgs>? TextChanging;

    /// <summary>
    ///  Occurs when an IME text composition changes.
    /// </summary>
    /// <remarks>Start index and length describe the active composition span in the current text buffer.</remarks>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionChanged;

    /// <summary>
    ///  Occurs when an IME text composition ends.
    /// </summary>
    /// <remarks>Raised after the platform reports composition completion for the reported text span.</remarks>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionEnded;

    /// <summary>
    ///  Occurs when an IME text composition starts.
    /// </summary>
    /// <remarks>Raised when the platform starts composing text at the reported span.</remarks>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionStarted;

    /// <summary>
    ///  Subscribes wrapper handlers to the currently hosted WinUI editor.
    /// </summary>
    private void AttachEditorEvents()
    {
        if (_textBox is not null)
        {
            _textBox.BeforeTextChanging += TextBoxBeforeTextChanging;
            _textBox.CandidateWindowBoundsChanged += TextBoxCandidateWindowBoundsChanged;
            _textBox.ContextMenuOpening += TextBoxContextMenuOpening;
            _textBox.CopyingToClipboard += TextBoxCopyingToClipboard;
            _textBox.CuttingToClipboard += TextBoxCuttingToClipboard;
            _textBox.Paste += TextBoxPaste;
            _textBox.SelectionChanged += TextBoxSelectionChanged;
            _textBox.SelectionChanging += TextBoxSelectionChanging;
            _textBox.TextChanged += TextBoxTextChanged;
            _textBox.TextChanging += TextBoxTextChanging;
            _textBox.TextCompositionChanged += TextBoxTextCompositionChanged;
            _textBox.TextCompositionEnded += TextBoxTextCompositionEnded;
            _textBox.TextCompositionStarted += TextBoxTextCompositionStarted;
            return;
        }

        XamlRichEditBox richEditBox = _richEditBox
            ?? throw new InvalidOperationException("The WinUI text editor was not created.");
        richEditBox.CandidateWindowBoundsChanged += RichEditBoxCandidateWindowBoundsChanged;
        richEditBox.ContextMenuOpening += RichEditBoxContextMenuOpening;
        richEditBox.CopyingToClipboard += RichEditBoxCopyingToClipboard;
        richEditBox.CuttingToClipboard += RichEditBoxCuttingToClipboard;
        richEditBox.Paste += RichEditBoxPaste;
        richEditBox.SelectionChanged += RichEditBoxSelectionChanged;
        richEditBox.SelectionChanging += RichEditBoxSelectionChanging;
        richEditBox.TextChanged += RichEditBoxTextChanged;
        richEditBox.TextChanging += RichEditBoxTextChanging;
        richEditBox.TextCompositionChanged += RichEditBoxTextCompositionChanged;
        richEditBox.TextCompositionEnded += RichEditBoxTextCompositionEnded;
        richEditBox.TextCompositionStarted += RichEditBoxTextCompositionStarted;
    }

    /// <summary>
    ///  Unsubscribes wrapper handlers from the currently hosted WinUI editor.
    /// </summary>
    private void DetachEditorEvents()
    {
        if (_textBox is not null)
        {
            _textBox.BeforeTextChanging -= TextBoxBeforeTextChanging;
            _textBox.CandidateWindowBoundsChanged -= TextBoxCandidateWindowBoundsChanged;
            _textBox.ContextMenuOpening -= TextBoxContextMenuOpening;
            _textBox.CopyingToClipboard -= TextBoxCopyingToClipboard;
            _textBox.CuttingToClipboard -= TextBoxCuttingToClipboard;
            _textBox.Paste -= TextBoxPaste;
            _textBox.SelectionChanged -= TextBoxSelectionChanged;
            _textBox.SelectionChanging -= TextBoxSelectionChanging;
            _textBox.TextChanged -= TextBoxTextChanged;
            _textBox.TextChanging -= TextBoxTextChanging;
            _textBox.TextCompositionChanged -= TextBoxTextCompositionChanged;
            _textBox.TextCompositionEnded -= TextBoxTextCompositionEnded;
            _textBox.TextCompositionStarted -= TextBoxTextCompositionStarted;
        }

        if (_richEditBox is not null)
        {
            _richEditBox.CandidateWindowBoundsChanged -= RichEditBoxCandidateWindowBoundsChanged;
            _richEditBox.ContextMenuOpening -= RichEditBoxContextMenuOpening;
            _richEditBox.CopyingToClipboard -= RichEditBoxCopyingToClipboard;
            _richEditBox.CuttingToClipboard -= RichEditBoxCuttingToClipboard;
            _richEditBox.Paste -= RichEditBoxPaste;
            _richEditBox.SelectionChanged -= RichEditBoxSelectionChanged;
            _richEditBox.SelectionChanging -= RichEditBoxSelectionChanging;
            _richEditBox.TextChanged -= RichEditBoxTextChanged;
            _richEditBox.TextChanging -= RichEditBoxTextChanging;
            _richEditBox.TextCompositionChanged -= RichEditBoxTextCompositionChanged;
            _richEditBox.TextCompositionEnded -= RichEditBoxTextCompositionEnded;
            _richEditBox.TextCompositionStarted -= RichEditBoxTextCompositionStarted;
        }
    }

    /// <summary>
    ///  Bridges TextBox before-change notifications into the wrapper cancellation contract.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI event arguments for the pending text update.</param>
    private void TextBoxBeforeTextChanging(XamlTextBox sender, XamlTextBoxBeforeTextChangingEventArgs eventArgs)
        => eventArgs.Cancel = OnBeforeTextChanging(eventArgs.NewText);

    /// <summary>
    ///  Allows derived wrappers to cancel a pending TextBox text commit.
    /// </summary>
    /// <param name="newText">The proposed full text value supplied by WinUI.</param>
    /// <returns><see langword="true"/> to cancel the commit; otherwise <see langword="false"/>.</returns>
    private protected virtual bool OnBeforeTextChanging(string newText) => false;

    /// <summary>
    ///  Receives candidate-window bounds changes from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI event arguments describing the candidate bounds.</param>
    private void TextBoxCandidateWindowBoundsChanged(
        XamlTextBox sender,
        XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => RaiseCandidateWindowBoundsChanged(eventArgs);

    /// <summary>
    ///  Receives candidate-window bounds changes from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI event arguments describing the candidate bounds.</param>
    private void RichEditBoxCandidateWindowBoundsChanged(
        XamlRichEditBox sender,
        XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => RaiseCandidateWindowBoundsChanged(eventArgs);

    /// <summary>
    ///  Projects candidate-window bounds into thirtytwo coordinates and raises the managed event.
    /// </summary>
    /// <param name="eventArgs">The WinUI bounds change payload.</param>
    private void RaiseCandidateWindowBoundsChanged(XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => CandidateWindowBoundsChanged?.Invoke(
            this,
            new(new RectangleF(
                Convert.ToSingle(eventArgs.Bounds.X),
                Convert.ToSingle(eventArgs.Bounds.Y),
                Convert.ToSingle(eventArgs.Bounds.Width),
                Convert.ToSingle(eventArgs.Bounds.Height))));

    /// <summary>
    ///  Receives context-menu opening notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The WinUI context-menu request payload.</param>
    private void TextBoxContextMenuOpening(object sender, XamlContextMenuEventArgs eventArgs)
        => RaiseContextMenuOpening(eventArgs);

    /// <summary>
    ///  Receives context-menu opening notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The WinUI context-menu request payload.</param>
    private void RichEditBoxContextMenuOpening(object sender, XamlContextMenuEventArgs eventArgs)
        => RaiseContextMenuOpening(eventArgs);

    /// <summary>
    ///  Raises the managed context-menu opening event and writes back handled state to WinUI.
    /// </summary>
    /// <param name="eventArgs">The WinUI context-menu request payload.</param>
    private void RaiseContextMenuOpening(XamlContextMenuEventArgs eventArgs)
    {
        WinUITextContextMenuOpeningEventArgs projected = new(
            new PointF(Convert.ToSingle(eventArgs.CursorLeft), Convert.ToSingle(eventArgs.CursorTop)));
        ContextMenuOpening?.Invoke(this, projected);
        eventArgs.Handled = projected.Handled;
    }

    /// <summary>
    ///  Receives copy-to-clipboard notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI copy event payload.</param>
    private void TextBoxCopyingToClipboard(XamlTextBox sender, XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => RaiseCopyingToClipboard(eventArgs);

    /// <summary>
    ///  Receives copy-to-clipboard notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI copy event payload.</param>
    private void RichEditBoxCopyingToClipboard(
        XamlRichEditBox sender,
        XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => RaiseCopyingToClipboard(eventArgs);

    /// <summary>
    ///  Raises the managed copy event and synchronizes handled state back to WinUI.
    /// </summary>
    /// <param name="eventArgs">The WinUI copy event payload.</param>
    private void RaiseCopyingToClipboard(XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, CopyingToClipboard, eventArgs.Handled);

    /// <summary>
    ///  Receives cut-to-clipboard notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI cut event payload.</param>
    private void TextBoxCuttingToClipboard(XamlTextBox sender, XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => RaiseCuttingToClipboard(eventArgs);

    /// <summary>
    ///  Receives cut-to-clipboard notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI cut event payload.</param>
    private void RichEditBoxCuttingToClipboard(
        XamlRichEditBox sender,
        XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => RaiseCuttingToClipboard(eventArgs);

    /// <summary>
    ///  Raises the managed cut event and synchronizes handled state back to WinUI.
    /// </summary>
    /// <param name="eventArgs">The WinUI cut event payload.</param>
    private void RaiseCuttingToClipboard(XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, CuttingToClipboard, eventArgs.Handled);

    /// <summary>
    ///  Receives paste notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The WinUI paste event payload.</param>
    private void TextBoxPaste(object sender, XamlTextControlPasteEventArgs eventArgs)
        => RaisePaste(eventArgs);

    /// <summary>
    ///  Receives paste notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The WinUI paste event payload.</param>
    private void RichEditBoxPaste(object sender, XamlTextControlPasteEventArgs eventArgs)
        => RaisePaste(eventArgs);

    /// <summary>
    ///  Raises the managed paste event and synchronizes handled state back to WinUI.
    /// </summary>
    /// <param name="eventArgs">The WinUI paste event payload.</param>
    private void RaisePaste(XamlTextControlPasteEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, Paste, eventArgs.Handled);

    /// <summary>
    ///  Raises a projected clipboard event and returns the final handled state.
    /// </summary>
    /// <param name="sender">The managed sender.</param>
    /// <param name="eventHandler">The subscribed managed clipboard handlers.</param>
    /// <param name="handled">The initial handled value from WinUI.</param>
    /// <returns>The handled value after managed handlers run.</returns>
    private static bool RaiseClipboardEvent(
        object sender,
        EventHandler<WinUITextClipboardEventArgs>? eventHandler,
        bool handled)
    {
        WinUITextClipboardEventArgs projected = new() { Handled = handled };
        eventHandler?.Invoke(sender, projected);
        return projected.Handled;
    }

    /// <summary>
    ///  Receives selection-changed notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The routed-event payload.</param>
    private void TextBoxSelectionChanged(object sender, RoutedEventArgs eventArgs)
        => SelectionChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///  Receives selection-changed notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The routed-event payload.</param>
    private void RichEditBoxSelectionChanged(object sender, RoutedEventArgs eventArgs)
        => SelectionChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///  Receives selection-changing notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI selection-change payload.</param>
    private void TextBoxSelectionChanging(XamlTextBox sender, XamlTextBoxSelectionChangingEventArgs eventArgs)
        => RaiseSelectionChanging(eventArgs.SelectionStart, eventArgs.SelectionLength, value => eventArgs.Cancel = value);

    /// <summary>
    ///  Receives selection-changing notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI selection-change payload.</param>
    private void RichEditBoxSelectionChanging(
        XamlRichEditBox sender,
        XamlRichEditBoxSelectionChangingEventArgs eventArgs)
        => RaiseSelectionChanging(eventArgs.SelectionStart, eventArgs.SelectionLength, value => eventArgs.Cancel = value);

    /// <summary>
    ///  Raises the projected selection-changing event and applies cancellation back to WinUI.
    /// </summary>
    /// <param name="selectionStart">The proposed zero-based selection start index.</param>
    /// <param name="selectionLength">The proposed selection length in characters.</param>
    /// <param name="setCancel">Assigns the projected cancellation value to WinUI event args.</param>
    private void RaiseSelectionChanging(int selectionStart, int selectionLength, Action<bool> setCancel)
    {
        WinUITextSelectionChangingEventArgs projected = new(selectionStart, selectionLength);
        SelectionChanging?.Invoke(this, projected);
        setCancel(projected.Cancel);
    }

    /// <summary>
    ///  Receives text-changed notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The text-changed payload.</param>
    private void TextBoxTextChanged(object sender, TextChangedEventArgs eventArgs)
        => TextChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///  Receives text-changed notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating element.</param>
    /// <param name="eventArgs">The routed-event payload.</param>
    private void RichEditBoxTextChanged(object sender, RoutedEventArgs eventArgs)
        => TextChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///  Receives text-changing notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI text-changing payload.</param>
    private void TextBoxTextChanging(XamlTextBox sender, XamlTextBoxTextChangingEventArgs eventArgs)
        => TextChanging?.Invoke(this, new WinUITextChangingEventArgs(eventArgs.IsContentChanging));

    /// <summary>
    ///  Receives text-changing notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI text-changing payload.</param>
    private void RichEditBoxTextChanging(XamlRichEditBox sender, XamlRichEditBoxTextChangingEventArgs eventArgs)
        => TextChanging?.Invoke(this, new WinUITextChangingEventArgs(eventArgs.IsContentChanging));

    /// <summary>
    ///  Receives text-composition changed notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void TextBoxTextCompositionChanged(XamlTextBox sender, XamlTextCompositionChangedEventArgs eventArgs)
        => RaiseTextCompositionChanged(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Receives text-composition changed notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void RichEditBoxTextCompositionChanged(XamlRichEditBox sender, XamlTextCompositionChangedEventArgs eventArgs)
        => RaiseTextCompositionChanged(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Raises the managed composition-changed event.
    /// </summary>
    /// <param name="startIndex">The zero-based start index of the composition span.</param>
    /// <param name="length">The composition span length in characters.</param>
    private void RaiseTextCompositionChanged(int startIndex, int length)
        => TextCompositionChanged?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));

    /// <summary>
    ///  Receives text-composition ended notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void TextBoxTextCompositionEnded(XamlTextBox sender, XamlTextCompositionEndedEventArgs eventArgs)
        => RaiseTextCompositionEnded(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Receives text-composition ended notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void RichEditBoxTextCompositionEnded(XamlRichEditBox sender, XamlTextCompositionEndedEventArgs eventArgs)
        => RaiseTextCompositionEnded(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Raises the managed composition-ended event.
    /// </summary>
    /// <param name="startIndex">The zero-based start index of the composition span.</param>
    /// <param name="length">The composition span length in characters.</param>
    private void RaiseTextCompositionEnded(int startIndex, int length)
        => TextCompositionEnded?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));

    /// <summary>
    ///  Receives text-composition started notifications from a TextBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI TextBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void TextBoxTextCompositionStarted(XamlTextBox sender, XamlTextCompositionStartedEventArgs eventArgs)
        => RaiseTextCompositionStarted(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Receives text-composition started notifications from a RichEditBox host.
    /// </summary>
    /// <param name="sender">The originating WinUI RichEditBox.</param>
    /// <param name="eventArgs">The WinUI composition payload.</param>
    private void RichEditBoxTextCompositionStarted(XamlRichEditBox sender, XamlTextCompositionStartedEventArgs eventArgs)
        => RaiseTextCompositionStarted(eventArgs.StartIndex, eventArgs.Length);

    /// <summary>
    ///  Raises the managed composition-started event.
    /// </summary>
    /// <param name="startIndex">The zero-based start index of the composition span.</param>
    /// <param name="length">The composition span length in characters.</param>
    private void RaiseTextCompositionStarted(int startIndex, int length)
        => TextCompositionStarted?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));
}
