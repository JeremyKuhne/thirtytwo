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
    /// <summary>Occurs when IME candidate-window bounds change.</summary>
    public event EventHandler<WinUICandidateWindowBoundsChangedEventArgs>? CandidateWindowBoundsChanged;

    /// <summary>Occurs when the editor requests its context menu.</summary>
    public event EventHandler<WinUITextContextMenuOpeningEventArgs>? ContextMenuOpening;

    /// <summary>Occurs before selected content is copied to the clipboard.</summary>
    public event EventHandler<WinUITextClipboardEventArgs>? CopyingToClipboard;

    /// <summary>Occurs before selected content is cut to the clipboard.</summary>
    public event EventHandler<WinUITextClipboardEventArgs>? CuttingToClipboard;

    /// <summary>Occurs before clipboard content is pasted.</summary>
    public event EventHandler<WinUITextClipboardEventArgs>? Paste;

    /// <summary>Occurs after the text selection changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Occurs before the text selection changes.</summary>
    public event EventHandler<WinUITextSelectionChangingEventArgs>? SelectionChanging;

    /// <summary>Occurs after editor text changes.</summary>
    public event EventHandler? TextChanged;

    /// <summary>Occurs synchronously while editor text is changing.</summary>
    public event EventHandler<WinUITextChangingEventArgs>? TextChanging;

    /// <summary>Occurs when an IME text composition changes.</summary>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionChanged;

    /// <summary>Occurs when an IME text composition ends.</summary>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionEnded;

    /// <summary>Occurs when an IME text composition starts.</summary>
    public event EventHandler<WinUITextCompositionEventArgs>? TextCompositionStarted;

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

    private void TextBoxBeforeTextChanging(XamlTextBox sender, XamlTextBoxBeforeTextChangingEventArgs eventArgs)
        => eventArgs.Cancel = OnBeforeTextChanging(eventArgs.NewText);

    private protected virtual bool OnBeforeTextChanging(string newText) => false;

    private void TextBoxCandidateWindowBoundsChanged(
        XamlTextBox sender,
        XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => RaiseCandidateWindowBoundsChanged(eventArgs);

    private void RichEditBoxCandidateWindowBoundsChanged(
        XamlRichEditBox sender,
        XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => RaiseCandidateWindowBoundsChanged(eventArgs);

    private void RaiseCandidateWindowBoundsChanged(XamlCandidateWindowBoundsChangedEventArgs eventArgs)
        => CandidateWindowBoundsChanged?.Invoke(
            this,
            new(new RectangleF(
                Convert.ToSingle(eventArgs.Bounds.X),
                Convert.ToSingle(eventArgs.Bounds.Y),
                Convert.ToSingle(eventArgs.Bounds.Width),
                Convert.ToSingle(eventArgs.Bounds.Height))));

    private void TextBoxContextMenuOpening(object sender, XamlContextMenuEventArgs eventArgs)
        => RaiseContextMenuOpening(eventArgs);

    private void RichEditBoxContextMenuOpening(object sender, XamlContextMenuEventArgs eventArgs)
        => RaiseContextMenuOpening(eventArgs);

    private void RaiseContextMenuOpening(XamlContextMenuEventArgs eventArgs)
    {
        WinUITextContextMenuOpeningEventArgs projected = new(
            new PointF(Convert.ToSingle(eventArgs.CursorLeft), Convert.ToSingle(eventArgs.CursorTop)));
        ContextMenuOpening?.Invoke(this, projected);
        eventArgs.Handled = projected.Handled;
    }

    private void TextBoxCopyingToClipboard(XamlTextBox sender, XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => RaiseCopyingToClipboard(eventArgs);

    private void RichEditBoxCopyingToClipboard(
        XamlRichEditBox sender,
        XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => RaiseCopyingToClipboard(eventArgs);

    private void RaiseCopyingToClipboard(XamlTextControlCopyingToClipboardEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, CopyingToClipboard, eventArgs.Handled);

    private void TextBoxCuttingToClipboard(XamlTextBox sender, XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => RaiseCuttingToClipboard(eventArgs);

    private void RichEditBoxCuttingToClipboard(
        XamlRichEditBox sender,
        XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => RaiseCuttingToClipboard(eventArgs);

    private void RaiseCuttingToClipboard(XamlTextControlCuttingToClipboardEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, CuttingToClipboard, eventArgs.Handled);

    private void TextBoxPaste(object sender, XamlTextControlPasteEventArgs eventArgs)
        => RaisePaste(eventArgs);

    private void RichEditBoxPaste(object sender, XamlTextControlPasteEventArgs eventArgs)
        => RaisePaste(eventArgs);

    private void RaisePaste(XamlTextControlPasteEventArgs eventArgs)
        => eventArgs.Handled = RaiseClipboardEvent(this, Paste, eventArgs.Handled);

    private static bool RaiseClipboardEvent(
        object sender,
        EventHandler<WinUITextClipboardEventArgs>? eventHandler,
        bool handled)
    {
        WinUITextClipboardEventArgs projected = new() { Handled = handled };
        eventHandler?.Invoke(sender, projected);
        return projected.Handled;
    }

    private void TextBoxSelectionChanged(object sender, RoutedEventArgs eventArgs)
        => SelectionChanged?.Invoke(this, EventArgs.Empty);

    private void RichEditBoxSelectionChanged(object sender, RoutedEventArgs eventArgs)
        => SelectionChanged?.Invoke(this, EventArgs.Empty);

    private void TextBoxSelectionChanging(XamlTextBox sender, XamlTextBoxSelectionChangingEventArgs eventArgs)
        => RaiseSelectionChanging(eventArgs.SelectionStart, eventArgs.SelectionLength, value => eventArgs.Cancel = value);

    private void RichEditBoxSelectionChanging(
        XamlRichEditBox sender,
        XamlRichEditBoxSelectionChangingEventArgs eventArgs)
        => RaiseSelectionChanging(eventArgs.SelectionStart, eventArgs.SelectionLength, value => eventArgs.Cancel = value);

    private void RaiseSelectionChanging(int selectionStart, int selectionLength, Action<bool> setCancel)
    {
        WinUITextSelectionChangingEventArgs projected = new(selectionStart, selectionLength);
        SelectionChanging?.Invoke(this, projected);
        setCancel(projected.Cancel);
    }

    private void TextBoxTextChanged(object sender, TextChangedEventArgs eventArgs)
        => TextChanged?.Invoke(this, EventArgs.Empty);

    private void RichEditBoxTextChanged(object sender, RoutedEventArgs eventArgs)
        => TextChanged?.Invoke(this, EventArgs.Empty);

    private void TextBoxTextChanging(XamlTextBox sender, XamlTextBoxTextChangingEventArgs eventArgs)
        => TextChanging?.Invoke(this, new WinUITextChangingEventArgs(eventArgs.IsContentChanging));

    private void RichEditBoxTextChanging(XamlRichEditBox sender, XamlRichEditBoxTextChangingEventArgs eventArgs)
        => TextChanging?.Invoke(this, new WinUITextChangingEventArgs(eventArgs.IsContentChanging));

    private void TextBoxTextCompositionChanged(XamlTextBox sender, XamlTextCompositionChangedEventArgs eventArgs)
        => RaiseTextCompositionChanged(eventArgs.StartIndex, eventArgs.Length);

    private void RichEditBoxTextCompositionChanged(XamlRichEditBox sender, XamlTextCompositionChangedEventArgs eventArgs)
        => RaiseTextCompositionChanged(eventArgs.StartIndex, eventArgs.Length);

    private void RaiseTextCompositionChanged(int startIndex, int length)
        => TextCompositionChanged?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));

    private void TextBoxTextCompositionEnded(XamlTextBox sender, XamlTextCompositionEndedEventArgs eventArgs)
        => RaiseTextCompositionEnded(eventArgs.StartIndex, eventArgs.Length);

    private void RichEditBoxTextCompositionEnded(XamlRichEditBox sender, XamlTextCompositionEndedEventArgs eventArgs)
        => RaiseTextCompositionEnded(eventArgs.StartIndex, eventArgs.Length);

    private void RaiseTextCompositionEnded(int startIndex, int length)
        => TextCompositionEnded?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));

    private void TextBoxTextCompositionStarted(XamlTextBox sender, XamlTextCompositionStartedEventArgs eventArgs)
        => RaiseTextCompositionStarted(eventArgs.StartIndex, eventArgs.Length);

    private void RichEditBoxTextCompositionStarted(XamlRichEditBox sender, XamlTextCompositionStartedEventArgs eventArgs)
        => RaiseTextCompositionStarted(eventArgs.StartIndex, eventArgs.Length);

    private void RaiseTextCompositionStarted(int startIndex, int length)
        => TextCompositionStarted?.Invoke(this, new WinUITextCompositionEventArgs(startIndex, length));
}
