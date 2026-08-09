// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.ExceptionServices;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using XamlCandidateWindowAlignment = Microsoft.UI.Xaml.Controls.CandidateWindowAlignment;
using XamlCharacterCasing = Microsoft.UI.Xaml.Controls.CharacterCasing;
using XamlControl = Microsoft.UI.Xaml.Controls.Control;
using XamlDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using XamlInputScope = Microsoft.UI.Xaml.Input.InputScope;
using XamlInputScopeName = Microsoft.UI.Xaml.Input.InputScopeName;
using XamlInputScopeNameValue = Microsoft.UI.Xaml.Input.InputScopeNameValue;
using XamlRichEditBox = Microsoft.UI.Xaml.Controls.RichEditBox;
using XamlTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using XamlTextBox = Microsoft.UI.Xaml.Controls.TextBox;
using XamlTextReadingOrder = Microsoft.UI.Xaml.TextReadingOrder;
using XamlTextWrapping = Microsoft.UI.Xaml.TextWrapping;

namespace Windows.WinUI;

/// <summary>Hosts a WinUI text editor and projects its common editing contract through .NET types.</summary>
/// <remarks>
///  <para>
///   Members must be accessed from the owner thread while the control is alive. Text, selection, clipboard, undo,
///   input-method, and theme behavior is provided by the hosted WinUI control rather than a native Edit HWND.
///  </para>
///  <para>
///   Editor-specific properties and events are projected here. XAML framework members are not duplicated; callers
///   that need them can use the <see cref="Content"/> surface inherited from <see cref="XamlHostControl"/>.
///  </para>
/// </remarks>
public abstract partial class WinUITextControl : XamlHostControl
{
    private XamlControl? _editor;
    private XamlTextBox? _textBox;
    private XamlRichEditBox? _richEditBox;

    private protected WinUITextControl(Rectangle bounds, Window parentWindow, bool richEdit)
        : base(bounds, parentWindow)
    {
        XamlControl? editor = null;
        try
        {
            editor = richEdit ? new XamlRichEditBox() : new XamlTextBox();
            _editor = editor;
            _textBox = editor as XamlTextBox;
            _richEditBox = editor as XamlRichEditBox;
            AttachEditorEvents();
            Content = editor;
        }
        catch (Exception constructionFailure)
        {
            ThrowAfterFailedConstruction(constructionFailure);
        }
    }

    /// <summary>Gets the hosted WinUI text editor.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is not the hosted editor.</exception>
    public override Microsoft.UI.Xaml.UIElement? Content
    {
        get => base.Content;
        set
        {
            if (!ReferenceEquals(value, _editor))
            {
                throw new InvalidOperationException("WinUITextControl content cannot be replaced.");
            }

            base.Content = value;
        }
    }

    /// <summary>Gets or sets the editor text.</summary>
    /// <remarks>The RichEdit document's synthetic final paragraph mark is not included in the returned text.</remarks>
    public new string Text
    {
        get
        {
            VerifyUsable();
            if (_textBox is not null)
            {
                return _textBox.Text;
            }

            _richEditBox!.Document.GetText(TextGetOptions.None, out string text);
            return text.Length > 0 && text[^1] == '\r' ? text[..^1] : text;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            VerifyUsable();
            if (_textBox is not null)
            {
                _textBox.Text = value;
            }
            else
            {
                _richEditBox!.Document.SetText(TextSetOptions.None, value);
            }
        }
    }

    /// <summary>Gets or sets whether the editor accepts newline input.</summary>
    public bool AcceptsReturn
    {
        get => GetCommon(static editor => editor.AcceptsReturn, static editor => editor.AcceptsReturn);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.AcceptsReturn = newValue,
            static (editor, newValue) => editor.AcceptsReturn = newValue);
    }

    /// <summary>Gets whether clipboard content can currently be pasted.</summary>
    public bool CanPasteClipboardContent
    {
        get
        {
            VerifyUsable();
            return _textBox?.CanPasteClipboardContent ?? _richEditBox!.Document.CanPaste();
        }
    }

    /// <summary>Gets whether a redo operation is available.</summary>
    public bool CanRedo
    {
        get
        {
            VerifyUsable();
            return _textBox?.CanRedo ?? _richEditBox!.Document.CanRedo();
        }
    }

    /// <summary>Gets whether an undo operation is available.</summary>
    public bool CanUndo
    {
        get
        {
            VerifyUsable();
            return _textBox?.CanUndo ?? _richEditBox!.Document.CanUndo();
        }
    }

    /// <summary>Gets or sets automatic character casing.</summary>
    public WinUITextCharacterCasing CharacterCasing
    {
        get => FromXaml(GetCommon(static editor => editor.CharacterCasing, static editor => editor.CharacterCasing));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.CharacterCasing = newValue,
            static (editor, newValue) => editor.CharacterCasing = newValue);
    }

    /// <summary>Gets or sets legacy descriptive text displayed by the editor.</summary>
    public string? Description
    {
        get => GetCommon(static editor => editor.Description?.ToString(), static editor => editor.Description?.ToString());
        set => SetCommon(
            value,
            static (editor, newValue) => editor.Description = newValue,
            static (editor, newValue) => editor.Description = newValue);
    }

    /// <summary>Gets or sets preferred IME candidate-window alignment.</summary>
    public WinUITextCandidateWindowAlignment DesiredCandidateWindowAlignment
    {
        get => FromXaml(GetCommon(
            static editor => editor.DesiredCandidateWindowAlignment,
            static editor => editor.DesiredCandidateWindowAlignment));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.DesiredCandidateWindowAlignment = newValue,
            static (editor, newValue) => editor.DesiredCandidateWindowAlignment = newValue);
    }

    /// <summary>Gets or sets the editor header.</summary>
    public object? Header
    {
        get => GetCommon(static editor => editor.Header, static editor => editor.Header);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.Header = newValue,
            static (editor, newValue) => editor.Header = newValue);
    }

    /// <summary>Gets or sets the editor header-template object.</summary>
    /// <remarks>A non-null value must be a WinUI DataTemplate.</remarks>
    public object? HeaderTemplate
    {
        get => GetCommon(static editor => editor.HeaderTemplate, static editor => editor.HeaderTemplate);
        set
        {
            if (value is not null and not XamlDataTemplate)
            {
                throw new ArgumentException("The header template must be a WinUI DataTemplate.", nameof(value));
            }

            XamlDataTemplate? template = (XamlDataTemplate?)value;
            SetCommon(
                template,
                static (editor, newValue) => editor.HeaderTemplate = newValue,
                static (editor, newValue) => editor.HeaderTemplate = newValue);
        }
    }

    /// <summary>Gets or sets horizontal alignment used during text layout.</summary>
    public WinUITextAlignment HorizontalTextAlignment
    {
        get => FromXaml(GetCommon(
            static editor => editor.HorizontalTextAlignment,
            static editor => editor.HorizontalTextAlignment));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.HorizontalTextAlignment = newValue,
            static (editor, newValue) => editor.HorizontalTextAlignment = newValue);
    }

    /// <summary>Gets or sets input-method scope hints.</summary>
    public IReadOnlyList<WinUITextInputScopeName> InputScope
    {
        get
        {
            XamlInputScope scope = GetCommon(static editor => editor.InputScope, static editor => editor.InputScope);
            return scope.Names.Select(name => (WinUITextInputScopeName)(int)name.NameValue).ToArray();
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            VerifyUsable();
            XamlInputScope scope = new();
            foreach (WinUITextInputScopeName name in value)
            {
                if (!Enum.IsDefined(name))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), name, "Unknown input-scope name.");
                }

                scope.Names.Add(new XamlInputScopeName { NameValue = (XamlInputScopeNameValue)(int)name });
            }

            SetCommon(
                scope,
                static (editor, newValue) => editor.InputScope = newValue,
                static (editor, newValue) => editor.InputScope = newValue);
        }
    }

    /// <summary>Gets or sets whether color-font glyphs are enabled.</summary>
    public bool IsColorFontEnabled
    {
        get => GetCommon(static editor => editor.IsColorFontEnabled, static editor => editor.IsColorFontEnabled);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.IsColorFontEnabled = newValue,
            static (editor, newValue) => editor.IsColorFontEnabled = newValue);
    }

    /// <summary>Gets or sets whether text is read-only.</summary>
    public bool IsReadOnly
    {
        get => GetCommon(static editor => editor.IsReadOnly, static editor => editor.IsReadOnly);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.IsReadOnly = newValue,
            static (editor, newValue) => editor.IsReadOnly = newValue);
    }

    /// <summary>Gets or sets whether spell checking is enabled.</summary>
    public bool IsSpellCheckEnabled
    {
        get => GetCommon(static editor => editor.IsSpellCheckEnabled, static editor => editor.IsSpellCheckEnabled);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.IsSpellCheckEnabled = newValue,
            static (editor, newValue) => editor.IsSpellCheckEnabled = newValue);
    }

    /// <summary>Gets or sets whether text prediction is enabled.</summary>
    public bool IsTextPredictionEnabled
    {
        get => GetCommon(static editor => editor.IsTextPredictionEnabled, static editor => editor.IsTextPredictionEnabled);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.IsTextPredictionEnabled = newValue,
            static (editor, newValue) => editor.IsTextPredictionEnabled = newValue);
    }

    /// <summary>Gets or sets the maximum text length. Zero uses the platform default.</summary>
    public int MaxLength
    {
        get => GetCommon(static editor => editor.MaxLength, static editor => editor.MaxLength);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.MaxLength = newValue,
            static (editor, newValue) => editor.MaxLength = newValue);
    }

    /// <summary>Gets or sets placeholder text.</summary>
    public string PlaceholderText
    {
        get => GetCommon(static editor => editor.PlaceholderText, static editor => editor.PlaceholderText);
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetCommon(
                value,
                static (editor, newValue) => editor.PlaceholderText = newValue,
                static (editor, newValue) => editor.PlaceholderText = newValue);
        }
    }

    /// <summary>Gets or sets whether programmatic focus suppresses automatic keyboard display.</summary>
    public bool PreventKeyboardDisplayOnProgrammaticFocus
    {
        get => GetCommon(
            static editor => editor.PreventKeyboardDisplayOnProgrammaticFocus,
            static editor => editor.PreventKeyboardDisplayOnProgrammaticFocus);
        set => SetCommon(
            value,
            static (editor, newValue) => editor.PreventKeyboardDisplayOnProgrammaticFocus = newValue,
            static (editor, newValue) => editor.PreventKeyboardDisplayOnProgrammaticFocus = newValue);
    }

    /// <summary>Gets whether the platform proofing menu is currently available.</summary>
    public bool ProofingMenuFlyout => GetCommon(
        static editor => editor.ProofingMenuFlyout is not null,
        static editor => editor.ProofingMenuFlyout is not null);

    /// <summary>Gets or sets selected text.</summary>
    public string SelectedText
    {
        get
        {
            VerifyUsable();
            return _textBox?.SelectedText ?? _richEditBox!.Document.Selection.Text;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            VerifyUsable();
            if (_textBox is not null)
            {
                _textBox.SelectedText = value;
            }
            else
            {
                _richEditBox!.Document.Selection.Text = value;
            }
        }
    }

    /// <summary>Gets or sets whether the standard text selection flyout is available.</summary>
    public WinUITextFlyoutMode SelectionFlyout
    {
        get => GetCommon(
            static editor => editor.SelectionFlyout is null
                ? WinUITextFlyoutMode.Disabled
                : WinUITextFlyoutMode.Default,
            static editor => editor.SelectionFlyout is null
                ? WinUITextFlyoutMode.Disabled
                : WinUITextFlyoutMode.Default);
        set
        {
            FlyoutBase? flyout = value switch
            {
                WinUITextFlyoutMode.Default => new TextCommandBarFlyout(),
                WinUITextFlyoutMode.Disabled => null,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown text flyout mode.")
            };
            SetCommon(
                flyout,
                static (editor, newValue) => editor.SelectionFlyout = newValue,
                static (editor, newValue) => editor.SelectionFlyout = newValue);
        }
    }

    /// <summary>Gets or sets focused selection highlight color.</summary>
    public Color SelectionHighlightColor
    {
        get => GetSolidColor(
            GetCommon(static editor => editor.SelectionHighlightColor, static editor => editor.SelectionHighlightColor),
            nameof(SelectionHighlightColor));
        set => SetCommon(
            ToBrush(value),
            static (editor, newValue) => editor.SelectionHighlightColor = newValue,
            static (editor, newValue) => editor.SelectionHighlightColor = newValue);
    }

    /// <summary>Gets or sets unfocused selection highlight color.</summary>
    public Color SelectionHighlightColorWhenNotFocused
    {
        get => GetSolidColor(
            GetCommon(
                static editor => editor.SelectionHighlightColorWhenNotFocused,
                static editor => editor.SelectionHighlightColorWhenNotFocused),
            nameof(SelectionHighlightColorWhenNotFocused));
        set => SetCommon(
            ToBrush(value),
            static (editor, newValue) => editor.SelectionHighlightColorWhenNotFocused = newValue,
            static (editor, newValue) => editor.SelectionHighlightColorWhenNotFocused = newValue);
    }

    /// <summary>Gets or sets selection length.</summary>
    public int SelectionLength
    {
        get
        {
            VerifyUsable();
            return _textBox?.SelectionLength ?? _richEditBox!.Document.Selection.Length;
        }
        set
        {
            VerifyUsable();
            Select(SelectionStart, value);
        }
    }

    /// <summary>Gets or sets selection start.</summary>
    public int SelectionStart
    {
        get
        {
            VerifyUsable();
            return _textBox?.SelectionStart ?? _richEditBox!.Document.Selection.StartPosition;
        }
        set
        {
            VerifyUsable();
            Select(value, SelectionLength);
        }
    }

    /// <summary>Gets or sets text alignment.</summary>
    public WinUITextAlignment TextAlignment
    {
        get => FromXaml(GetCommon(static editor => editor.TextAlignment, static editor => editor.TextAlignment));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.TextAlignment = newValue,
            static (editor, newValue) => editor.TextAlignment = newValue);
    }

    /// <summary>Gets or sets text reading order.</summary>
    public WinUITextReadingOrder TextReadingOrder
    {
        get => FromXaml(GetCommon(static editor => editor.TextReadingOrder, static editor => editor.TextReadingOrder));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.TextReadingOrder = newValue,
            static (editor, newValue) => editor.TextReadingOrder = newValue);
    }

    /// <summary>Gets or sets text wrapping.</summary>
    public WinUITextWrapping TextWrapping
    {
        get => FromXaml(GetCommon(static editor => editor.TextWrapping, static editor => editor.TextWrapping));
        set => SetCommon(
            ToXaml(value),
            static (editor, newValue) => editor.TextWrapping = newValue,
            static (editor, newValue) => editor.TextWrapping = newValue);
    }

    /// <summary>Gets or sets the editor background color.</summary>
    public Color BackgroundColor
    {
        get => GetSolidColor(GetEditor().Background, nameof(BackgroundColor));
        set
        {
            VerifyUsable();
            GetEditor().Background = ToBrush(value);
        }
    }

    /// <summary>Gets or sets the editor foreground color.</summary>
    public Color ForegroundColor
    {
        get => GetSolidColor(GetEditor().Foreground, nameof(ForegroundColor));
        set
        {
            VerifyUsable();
            GetEditor().Foreground = ToBrush(value);
        }
    }

    /// <summary>Gets or sets the editor font family name.</summary>
    public string FontFamilyName
    {
        get
        {
            VerifyUsable();
            return GetEditor().FontFamily.Source;
        }
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            VerifyUsable();
            GetEditor().FontFamily = new FontFamily(value);
        }
    }

    /// <summary>Gets or sets the editor font size in view pixels.</summary>
    public double FontSize
    {
        get
        {
            VerifyUsable();
            return GetEditor().FontSize;
        }
        set
        {
            VerifyUsable();
            GetEditor().FontSize = value;
        }
    }

    /// <summary>Gets or sets whether the editor is enabled.</summary>
    public bool IsEnabled
    {
        get
        {
            VerifyUsable();
            return GetEditor().IsEnabled;
        }
        set
        {
            VerifyUsable();
            GetEditor().IsEnabled = value;
            _ = PInvoke.EnableWindow(Handle, value);
        }
    }

    /// <summary>Gets or sets whether the editor participates in tab navigation.</summary>
    public bool IsTabStop
    {
        get
        {
            VerifyUsable();
            return GetEditor().IsTabStop;
        }
        set
        {
            VerifyUsable();
            GetEditor().IsTabStop = value;
        }
    }

    /// <summary>Gets or sets the editor tab order.</summary>
    public int TabIndex
    {
        get
        {
            VerifyUsable();
            return GetEditor().TabIndex;
        }
        set
        {
            VerifyUsable();
            GetEditor().TabIndex = value;
        }
    }

    /// <summary>Gets or sets the theme requested for the editor.</summary>
    public WinUIElementTheme RequestedTheme
    {
        get
        {
            VerifyUsable();
            return GetEditor().RequestedTheme switch
            {
                Microsoft.UI.Xaml.ElementTheme.Default => WinUIElementTheme.Default,
                Microsoft.UI.Xaml.ElementTheme.Light => WinUIElementTheme.Light,
                Microsoft.UI.Xaml.ElementTheme.Dark => WinUIElementTheme.Dark,
                _ => throw new InvalidOperationException("The text editor returned an unknown requested theme.")
            };
        }
        set
        {
            VerifyUsable();
            GetEditor().RequestedTheme = value switch
            {
                WinUIElementTheme.Default => Microsoft.UI.Xaml.ElementTheme.Default,
                WinUIElementTheme.Light => Microsoft.UI.Xaml.ElementTheme.Light,
                WinUIElementTheme.Dark => Microsoft.UI.Xaml.ElementTheme.Dark,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown WinUI element theme.")
            };
        }
    }

    /// <summary>Clears the undo and redo history.</summary>
    public void ClearUndoRedoHistory()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.ClearUndoRedoHistory();
        }
        else
        {
            _richEditBox!.Document.ClearUndoRedoHistory();
        }
    }

    /// <summary>Copies the selection to the clipboard.</summary>
    public void CopySelectionToClipboard()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.CopySelectionToClipboard();
        }
        else
        {
            _richEditBox!.Document.Selection.Copy();
        }
    }

    /// <summary>Cuts the selection to the clipboard.</summary>
    public void CutSelectionToClipboard()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.CutSelectionToClipboard();
        }
        else
        {
            _richEditBox!.Document.Selection.Cut();
        }
    }

    /// <summary>Gets linguistic alternatives for the current text.</summary>
    public async Task<IReadOnlyList<string>> GetLinguisticAlternativesAsync(CancellationToken cancellationToken = default)
    {
        VerifyUsable();
        IReadOnlyList<string> alternatives = await (_textBox?.GetLinguisticAlternativesAsync()
            ?? _richEditBox!.GetLinguisticAlternativesAsync()).AsTask(cancellationToken);
        return alternatives;
    }

    /// <summary>Pastes clipboard text into the selection.</summary>
    public void PasteFromClipboard()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.PasteFromClipboard();
        }
        else
        {
            _richEditBox!.Document.Selection.Paste(0);
        }
    }

    /// <summary>Redoes the most recently undone operation.</summary>
    public void Redo()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.Redo();
        }
        else
        {
            _richEditBox!.Document.Redo();
        }
    }

    /// <summary>Selects a text range.</summary>
    public void Select(int start, int length)
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.Select(start, length);
        }
        else
        {
            _richEditBox!.Document.Selection.SetRange(start, checked(start + length));
        }
    }

    /// <summary>Selects all text.</summary>
    public void SelectAll()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.SelectAll();
        }
        else
        {
            ITextSelection selection = _richEditBox!.Document.Selection;
            selection.SetRange(0, selection.StoryLength);
        }
    }

    /// <summary>Undoes the most recent operation.</summary>
    public void Undo()
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            _textBox.Undo();
        }
        else
        {
            _richEditBox!.Document.Undo();
        }
    }

    internal XamlTextBox GetTextBox()
    {
        VerifyUsable();
        return _textBox ?? throw new InvalidOperationException("This wrapper does not host a TextBox.");
    }

    internal XamlRichEditBox GetRichEditBox()
    {
        VerifyUsable();
        return _richEditBox ?? throw new InvalidOperationException("This wrapper does not host a RichEditBox.");
    }

    internal void VerifyUsable()
    {
        VerifyAccess();
        ObjectDisposedException.ThrowIf(IsXamlSourceDisposed, this);
        ObjectDisposedException.ThrowIf(_editor is null, this);
    }

    /// <inheritdoc/>
    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if (message == MessageType.Destroy)
        {
            try
            {
                DetachEditor();
            }
            catch (Exception exception)
            {
                ReportNativeCallbackFailure("TextControlDestroy", exception);
            }
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing: false);
            return;
        }

        VerifyAccess();
        Exception? editorFailure = null;
        try
        {
            DetachEditor();
        }
        catch (Exception exception)
        {
            editorFailure = exception;
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception windowFailure) when (editorFailure is not null)
        {
            throw new AggregateException(editorFailure, windowFailure);
        }

        if (editorFailure is not null)
        {
            ExceptionDispatchInfo.Capture(editorFailure).Throw();
        }
    }

    private XamlControl GetEditor()
    {
        VerifyUsable();
        return _editor!;
    }

    private T GetCommon<T>(Func<XamlTextBox, T> textBoxGetter, Func<XamlRichEditBox, T> richEditBoxGetter)
    {
        VerifyUsable();
        return _textBox is not null ? textBoxGetter(_textBox) : richEditBoxGetter(_richEditBox!);
    }

    private void SetCommon<T>(
        T value,
        Action<XamlTextBox, T> textBoxSetter,
        Action<XamlRichEditBox, T> richEditBoxSetter)
    {
        VerifyUsable();
        if (_textBox is not null)
        {
            textBoxSetter(_textBox, value);
        }
        else
        {
            richEditBoxSetter(_richEditBox!, value);
        }
    }

    private void DetachEditor()
    {
        if (_editor is null)
        {
            return;
        }

        DetachEditorEvents();
        _editor = null;
        _textBox = null;
        _richEditBox = null;
    }

    [DoesNotReturn]
    private void ThrowAfterFailedConstruction(Exception constructionFailure)
    {
        List<Exception>? cleanupFailures = null;
        try
        {
            DetachEditor();
        }
        catch (Exception eventCleanupFailure)
        {
            cleanupFailures = [eventCleanupFailure];
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception windowFailure)
        {
            (cleanupFailures ??= []).Add(windowFailure);
        }

        if (cleanupFailures is not null)
        {
            cleanupFailures.Insert(0, constructionFailure);
            throw new AggregateException("WinUI text control construction and cleanup failed.", cleanupFailures);
        }

        ExceptionDispatchInfo.Capture(constructionFailure).Throw();
        throw new UnreachableException();
    }

    private protected static Color GetSolidColor(Brush? brush, string propertyName)
        => brush is SolidColorBrush solidColorBrush
            ? Color.FromArgb(
                solidColorBrush.Color.A,
                solidColorBrush.Color.R,
                solidColorBrush.Color.G,
                solidColorBrush.Color.B)
            : throw new InvalidOperationException($"The editor's {propertyName} is not a solid color.");

    private protected static SolidColorBrush ToBrush(Color color)
        => new(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));

    private static WinUITextCharacterCasing FromXaml(XamlCharacterCasing value) => value switch
    {
        XamlCharacterCasing.Normal => WinUITextCharacterCasing.Normal,
        XamlCharacterCasing.Lower => WinUITextCharacterCasing.Lower,
        XamlCharacterCasing.Upper => WinUITextCharacterCasing.Upper,
        _ => throw new InvalidOperationException("The editor returned unknown character casing.")
    };

    private static XamlCharacterCasing ToXaml(WinUITextCharacterCasing value) => value switch
    {
        WinUITextCharacterCasing.Normal => XamlCharacterCasing.Normal,
        WinUITextCharacterCasing.Lower => XamlCharacterCasing.Lower,
        WinUITextCharacterCasing.Upper => XamlCharacterCasing.Upper,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown character casing.")
    };

    private static WinUITextCandidateWindowAlignment FromXaml(XamlCandidateWindowAlignment value) => value switch
    {
        XamlCandidateWindowAlignment.Default => WinUITextCandidateWindowAlignment.Default,
        XamlCandidateWindowAlignment.BottomEdge => WinUITextCandidateWindowAlignment.BottomEdge,
        _ => throw new InvalidOperationException("The editor returned unknown candidate-window alignment.")
    };

    private static XamlCandidateWindowAlignment ToXaml(WinUITextCandidateWindowAlignment value) => value switch
    {
        WinUITextCandidateWindowAlignment.Default => XamlCandidateWindowAlignment.Default,
        WinUITextCandidateWindowAlignment.BottomEdge => XamlCandidateWindowAlignment.BottomEdge,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown candidate-window alignment.")
    };

    private static WinUITextAlignment FromXaml(XamlTextAlignment value) => value switch
    {
        XamlTextAlignment.Center => WinUITextAlignment.Center,
        XamlTextAlignment.Left => WinUITextAlignment.Left,
        XamlTextAlignment.Right => WinUITextAlignment.Right,
        XamlTextAlignment.Justify => WinUITextAlignment.Justify,
        XamlTextAlignment.DetectFromContent => WinUITextAlignment.DetectFromContent,
        _ => throw new InvalidOperationException("The editor returned unknown text alignment.")
    };

    private static XamlTextAlignment ToXaml(WinUITextAlignment value) => value switch
    {
        WinUITextAlignment.Center => XamlTextAlignment.Center,
        WinUITextAlignment.Left => XamlTextAlignment.Left,
        WinUITextAlignment.Right => XamlTextAlignment.Right,
        WinUITextAlignment.Justify => XamlTextAlignment.Justify,
        WinUITextAlignment.DetectFromContent => XamlTextAlignment.DetectFromContent,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown text alignment.")
    };

    private static WinUITextReadingOrder FromXaml(XamlTextReadingOrder value) => value switch
    {
        XamlTextReadingOrder.Default => WinUITextReadingOrder.Default,
        XamlTextReadingOrder.DetectFromContent => WinUITextReadingOrder.DetectFromContent,
        _ => throw new InvalidOperationException("The editor returned unknown text reading order.")
    };

    private static XamlTextReadingOrder ToXaml(WinUITextReadingOrder value) => value switch
    {
        WinUITextReadingOrder.Default => XamlTextReadingOrder.Default,
        WinUITextReadingOrder.DetectFromContent => XamlTextReadingOrder.DetectFromContent,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown text reading order.")
    };

    private static WinUITextWrapping FromXaml(XamlTextWrapping value) => value switch
    {
        XamlTextWrapping.NoWrap => WinUITextWrapping.NoWrap,
        XamlTextWrapping.Wrap => WinUITextWrapping.Wrap,
        XamlTextWrapping.WrapWholeWords => WinUITextWrapping.WrapWholeWords,
        _ => throw new InvalidOperationException("The editor returned unknown text wrapping.")
    };

    private static XamlTextWrapping ToXaml(WinUITextWrapping value) => value switch
    {
        WinUITextWrapping.NoWrap => XamlTextWrapping.NoWrap,
        WinUITextWrapping.Wrap => XamlTextWrapping.Wrap,
        WinUITextWrapping.WrapWholeWords => XamlTextWrapping.WrapWholeWords,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown text wrapping.")
    };

}