// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using FluentAssertions;
using Microsoft.UI.Xaml.Media;
using Touki.TestSupport;
using XamlCandidateWindowAlignment = Microsoft.UI.Xaml.Controls.CandidateWindowAlignment;
using XamlCharacterCasing = Microsoft.UI.Xaml.Controls.CharacterCasing;
using XamlDisabledFormattingAccelerators = Microsoft.UI.Xaml.Controls.DisabledFormattingAccelerators;
using XamlInputScopeNameValue = Microsoft.UI.Xaml.Input.InputScopeNameValue;
using XamlRichEditBox = Microsoft.UI.Xaml.Controls.RichEditBox;
using XamlRichEditClipboardFormat = Microsoft.UI.Xaml.Controls.RichEditClipboardFormat;
using XamlTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using XamlTextBox = Microsoft.UI.Xaml.Controls.TextBox;
using XamlTextReadingOrder = Microsoft.UI.Xaml.TextReadingOrder;
using XamlTextWrapping = Microsoft.UI.Xaml.TextWrapping;

namespace Windows.WinUI.Tests;

[TestClass]
public class TextEditorProjectionTests
{
    private const BindingFlags DeclaredPublicInstance =
        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    private const BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;

    private static readonly IReadOnlyDictionary<string, string> s_textBoxProperties =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [nameof(XamlTextBox.AcceptsReturn)] = nameof(WinUITextBox.AcceptsReturn),
            [nameof(XamlTextBox.CanPasteClipboardContent)] = nameof(WinUITextBox.CanPasteClipboardContent),
            [nameof(XamlTextBox.CanRedo)] = nameof(WinUITextBox.CanRedo),
            [nameof(XamlTextBox.CanUndo)] = nameof(WinUITextBox.CanUndo),
            [nameof(XamlTextBox.CharacterCasing)] = nameof(WinUITextBox.CharacterCasing),
            [nameof(XamlTextBox.Description)] = nameof(WinUITextBox.Description),
            [nameof(XamlTextBox.DesiredCandidateWindowAlignment)] = nameof(WinUITextBox.DesiredCandidateWindowAlignment),
            [nameof(XamlTextBox.Header)] = nameof(WinUITextBox.Header),
            [nameof(XamlTextBox.HeaderTemplate)] = nameof(WinUITextBox.HeaderTemplate),
            [nameof(XamlTextBox.HorizontalTextAlignment)] = nameof(WinUITextBox.HorizontalTextAlignment),
            [nameof(XamlTextBox.InputScope)] = nameof(WinUITextBox.InputScope),
            [nameof(XamlTextBox.IsColorFontEnabled)] = nameof(WinUITextBox.IsColorFontEnabled),
            [nameof(XamlTextBox.IsReadOnly)] = nameof(WinUITextBox.IsReadOnly),
            [nameof(XamlTextBox.IsSpellCheckEnabled)] = nameof(WinUITextBox.IsSpellCheckEnabled),
            [nameof(XamlTextBox.IsTextPredictionEnabled)] = nameof(WinUITextBox.IsTextPredictionEnabled),
            [nameof(XamlTextBox.MaxLength)] = nameof(WinUITextBox.MaxLength),
            [nameof(XamlTextBox.PlaceholderForeground)] = nameof(WinUITextBox.PlaceholderForegroundColor),
            [nameof(XamlTextBox.PlaceholderText)] = nameof(WinUITextBox.PlaceholderText),
            [nameof(XamlTextBox.PreventKeyboardDisplayOnProgrammaticFocus)] = nameof(WinUITextBox.PreventKeyboardDisplayOnProgrammaticFocus),
            [nameof(XamlTextBox.ProofingMenuFlyout)] = nameof(WinUITextBox.ProofingMenuFlyout),
            [nameof(XamlTextBox.SelectedText)] = nameof(WinUITextBox.SelectedText),
            [nameof(XamlTextBox.SelectionFlyout)] = nameof(WinUITextBox.SelectionFlyout),
            [nameof(XamlTextBox.SelectionHighlightColor)] = nameof(WinUITextBox.SelectionHighlightColor),
            [nameof(XamlTextBox.SelectionHighlightColorWhenNotFocused)] = nameof(WinUITextBox.SelectionHighlightColorWhenNotFocused),
            [nameof(XamlTextBox.SelectionLength)] = nameof(WinUITextBox.SelectionLength),
            [nameof(XamlTextBox.SelectionStart)] = nameof(WinUITextBox.SelectionStart),
            [nameof(XamlTextBox.Text)] = nameof(WinUITextBox.Text),
            [nameof(XamlTextBox.TextAlignment)] = nameof(WinUITextBox.TextAlignment),
            [nameof(XamlTextBox.TextReadingOrder)] = nameof(WinUITextBox.TextReadingOrder),
            [nameof(XamlTextBox.TextWrapping)] = nameof(WinUITextBox.TextWrapping)
        };

    private static readonly IReadOnlyDictionary<string, string> s_richEditBoxProperties =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [nameof(XamlRichEditBox.AcceptsReturn)] = nameof(WinUIRichEditBox.AcceptsReturn),
            [nameof(XamlRichEditBox.CharacterCasing)] = nameof(WinUIRichEditBox.CharacterCasing),
            [nameof(XamlRichEditBox.ClipboardCopyFormat)] = nameof(WinUIRichEditBox.ClipboardCopyFormat),
            [nameof(XamlRichEditBox.Description)] = nameof(WinUIRichEditBox.Description),
            [nameof(XamlRichEditBox.DesiredCandidateWindowAlignment)] = nameof(WinUIRichEditBox.DesiredCandidateWindowAlignment),
            [nameof(XamlRichEditBox.DisabledFormattingAccelerators)] = nameof(WinUIRichEditBox.DisabledFormattingAccelerators),
            [nameof(XamlRichEditBox.Document)] = nameof(WinUIRichEditBox.Document),
            [nameof(XamlRichEditBox.Header)] = nameof(WinUIRichEditBox.Header),
            [nameof(XamlRichEditBox.HeaderTemplate)] = nameof(WinUIRichEditBox.HeaderTemplate),
            [nameof(XamlRichEditBox.HorizontalTextAlignment)] = nameof(WinUIRichEditBox.HorizontalTextAlignment),
            [nameof(XamlRichEditBox.InputScope)] = nameof(WinUIRichEditBox.InputScope),
            [nameof(XamlRichEditBox.IsColorFontEnabled)] = nameof(WinUIRichEditBox.IsColorFontEnabled),
            [nameof(XamlRichEditBox.IsReadOnly)] = nameof(WinUIRichEditBox.IsReadOnly),
            [nameof(XamlRichEditBox.IsSpellCheckEnabled)] = nameof(WinUIRichEditBox.IsSpellCheckEnabled),
            [nameof(XamlRichEditBox.IsTextPredictionEnabled)] = nameof(WinUIRichEditBox.IsTextPredictionEnabled),
            [nameof(XamlRichEditBox.MaxLength)] = nameof(WinUIRichEditBox.MaxLength),
            [nameof(XamlRichEditBox.PlaceholderText)] = nameof(WinUIRichEditBox.PlaceholderText),
            [nameof(XamlRichEditBox.PreventKeyboardDisplayOnProgrammaticFocus)] = nameof(WinUIRichEditBox.PreventKeyboardDisplayOnProgrammaticFocus),
            [nameof(XamlRichEditBox.ProofingMenuFlyout)] = nameof(WinUIRichEditBox.ProofingMenuFlyout),
            [nameof(XamlRichEditBox.SelectionFlyout)] = nameof(WinUIRichEditBox.SelectionFlyout),
            [nameof(XamlRichEditBox.SelectionHighlightColor)] = nameof(WinUIRichEditBox.SelectionHighlightColor),
            [nameof(XamlRichEditBox.SelectionHighlightColorWhenNotFocused)] = nameof(WinUIRichEditBox.SelectionHighlightColorWhenNotFocused),
            [nameof(XamlRichEditBox.TextAlignment)] = nameof(WinUIRichEditBox.TextAlignment),
            [nameof(XamlRichEditBox.TextDocument)] = nameof(WinUIRichEditBox.TextDocument),
            [nameof(XamlRichEditBox.TextReadingOrder)] = nameof(WinUIRichEditBox.TextReadingOrder),
            [nameof(XamlRichEditBox.TextWrapping)] = nameof(WinUIRichEditBox.TextWrapping)
        };

    private static readonly string[] s_textBoxEvents =
    [
        nameof(XamlTextBox.BeforeTextChanging),
        nameof(XamlTextBox.CandidateWindowBoundsChanged),
        nameof(XamlTextBox.ContextMenuOpening),
        nameof(XamlTextBox.CopyingToClipboard),
        nameof(XamlTextBox.CuttingToClipboard),
        nameof(XamlTextBox.Paste),
        nameof(XamlTextBox.SelectionChanged),
        nameof(XamlTextBox.SelectionChanging),
        nameof(XamlTextBox.TextChanged),
        nameof(XamlTextBox.TextChanging),
        nameof(XamlTextBox.TextCompositionChanged),
        nameof(XamlTextBox.TextCompositionEnded),
        nameof(XamlTextBox.TextCompositionStarted)
    ];

    private static readonly string[] s_richEditBoxEvents =
    [
        nameof(XamlRichEditBox.CandidateWindowBoundsChanged),
        nameof(XamlRichEditBox.ContextMenuOpening),
        nameof(XamlRichEditBox.CopyingToClipboard),
        nameof(XamlRichEditBox.CuttingToClipboard),
        nameof(XamlRichEditBox.Paste),
        nameof(XamlRichEditBox.SelectionChanged),
        nameof(XamlRichEditBox.SelectionChanging),
        nameof(XamlRichEditBox.TextChanged),
        nameof(XamlRichEditBox.TextChanging),
        nameof(XamlRichEditBox.TextCompositionChanged),
        nameof(XamlRichEditBox.TextCompositionEnded),
        nameof(XamlRichEditBox.TextCompositionStarted)
    ];

    [TestMethod]
    public void TextBox_DeclaredPropertiesAndEvents_AreProjected()
    {
        AssertProperties(typeof(XamlTextBox), typeof(WinUITextBox), s_textBoxProperties);
        AssertEvents(typeof(XamlTextBox), typeof(WinUITextBox), s_textBoxEvents);
    }

    [TestMethod]
    public void RichEditBox_DeclaredPropertiesAndEvents_AreProjected()
    {
        AssertProperties(typeof(XamlRichEditBox), typeof(WinUIRichEditBox), s_richEditBoxProperties);
        AssertEvents(typeof(XamlRichEditBox), typeof(WinUIRichEditBox), s_richEditBoxEvents);
        typeof(WinUIRichEditBox).GetEvent(nameof(WinUITextBox.BeforeTextChanging), PublicInstance).Should().BeNull();
    }

    [TestMethod]
    public void TextWrappers_DoNotDeclareNewXamlTypedMembers()
    {
        typeof(WinUITextControl).BaseType.Should().Be(typeof(XamlHostControl));
        typeof(WinUITextBox).BaseType.Should().Be(typeof(WinUITextControl));
        typeof(WinUIRichEditBox).BaseType.Should().Be(typeof(WinUITextControl));

        List<string> xamlTypedMembers = [];
        foreach (Type wrapperType in new[] { typeof(WinUITextControl), typeof(WinUITextBox), typeof(WinUIRichEditBox) })
        {
            foreach (PropertyInfo property in wrapperType.GetProperties(DeclaredPublicInstance))
            {
                MethodInfo? accessor = property.GetMethod ?? property.SetMethod;
                if (accessor?.GetBaseDefinition() != accessor)
                {
                    continue;
                }

                AddIfXamlTyped(xamlTypedMembers, wrapperType, property, property.PropertyType);
            }

            foreach (EventInfo @event in wrapperType.GetEvents(DeclaredPublicInstance))
            {
                AddIfXamlTyped(xamlTypedMembers, wrapperType, @event, @event.EventHandlerType!);
            }

            foreach (MethodInfo method in wrapperType.GetMethods(DeclaredPublicInstance).Where(method => !method.IsSpecialName))
            {
                AddIfXamlTyped(xamlTypedMembers, wrapperType, method, method.ReturnType);
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    AddIfXamlTyped(xamlTypedMembers, wrapperType, method, parameter.ParameterType);
                }
            }

            foreach (ConstructorInfo constructor in wrapperType.GetConstructors(DeclaredPublicInstance))
            {
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    AddIfXamlTyped(xamlTypedMembers, wrapperType, constructor, parameter.ParameterType);
                }
            }
        }

        xamlTypedMembers.Should().BeEmpty();
    }

    [TestMethod]
    public void ProjectedEnums_MatchPinnedWinUIValues()
    {
        AssertSameValue(WinUITextCharacterCasing.Normal, XamlCharacterCasing.Normal);
        AssertSameValue(WinUITextCharacterCasing.Lower, XamlCharacterCasing.Lower);
        AssertSameValue(WinUITextCharacterCasing.Upper, XamlCharacterCasing.Upper);
        AssertSameValue(WinUITextCandidateWindowAlignment.Default, XamlCandidateWindowAlignment.Default);
        AssertSameValue(WinUITextCandidateWindowAlignment.BottomEdge, XamlCandidateWindowAlignment.BottomEdge);
        AssertSameValue(WinUITextAlignment.Center, XamlTextAlignment.Center);
        AssertSameValue(WinUITextAlignment.Left, XamlTextAlignment.Left);
        AssertSameValue(WinUITextAlignment.Start, XamlTextAlignment.Start);
        AssertSameValue(WinUITextAlignment.Right, XamlTextAlignment.Right);
        AssertSameValue(WinUITextAlignment.End, XamlTextAlignment.End);
        AssertSameValue(WinUITextAlignment.Justify, XamlTextAlignment.Justify);
        AssertSameValue(WinUITextAlignment.DetectFromContent, XamlTextAlignment.DetectFromContent);
        AssertSameValue(WinUITextReadingOrder.Default, XamlTextReadingOrder.Default);
        AssertSameValue(WinUITextReadingOrder.UseFlowDirection, XamlTextReadingOrder.UseFlowDirection);
        AssertSameValue(WinUITextReadingOrder.DetectFromContent, XamlTextReadingOrder.DetectFromContent);
        AssertSameValue(WinUITextWrapping.NoWrap, XamlTextWrapping.NoWrap);
        AssertSameValue(WinUITextWrapping.Wrap, XamlTextWrapping.Wrap);
        AssertSameValue(WinUITextWrapping.WrapWholeWords, XamlTextWrapping.WrapWholeWords);
        AssertSameValue(WinUIRichEditClipboardFormat.AllFormats, XamlRichEditClipboardFormat.AllFormats);
        AssertSameValue(WinUIRichEditClipboardFormat.PlainText, XamlRichEditClipboardFormat.PlainText);

        foreach (XamlInputScopeNameValue xamlValue in Enum.GetValues<XamlInputScopeNameValue>())
        {
            WinUITextInputScopeName projectedValue = Enum.Parse<WinUITextInputScopeName>(xamlValue.ToString());
            ((int)projectedValue).Should().Be((int)xamlValue);
        }

        AssertSameValue(
            WinUIRichEditDisabledFormattingAccelerators.None,
            XamlDisabledFormattingAccelerators.None);
        AssertSameValue(
            WinUIRichEditDisabledFormattingAccelerators.Bold,
            XamlDisabledFormattingAccelerators.Bold);
        AssertSameValue(
            WinUIRichEditDisabledFormattingAccelerators.Italic,
            XamlDisabledFormattingAccelerators.Italic);
        AssertSameValue(
            WinUIRichEditDisabledFormattingAccelerators.Underline,
            XamlDisabledFormattingAccelerators.Underline);
        unchecked((uint)(int)WinUIRichEditDisabledFormattingAccelerators.All).Should().Be(
            (uint)XamlDisabledFormattingAccelerators.All);
    }

    [TestMethod]
    public void ClipboardBridge_CopiesHandledValueBothWays()
    {
        Func<object, EventHandler<WinUITextClipboardEventArgs>?, bool, bool> raiseClipboardEvent =
            typeof(WinUITextControl).TestAccessor.CreateDelegate<
                Func<object, EventHandler<WinUITextClipboardEventArgs>?, bool, bool>>("RaiseClipboardEvent");
        object sender = new();
        bool initialHandled = false;
        EventHandler<WinUITextClipboardEventArgs> handle = (actualSender, eventArgs) =>
        {
            actualSender.Should().BeSameAs(sender);
            initialHandled = eventArgs.Handled;
            eventArgs.Handled = true;
        };

        raiseClipboardEvent(sender, handle, false).Should().BeTrue();
        initialHandled.Should().BeFalse();

        EventHandler<WinUITextClipboardEventArgs> clear = (_, eventArgs) => eventArgs.Handled = false;
        raiseClipboardEvent(sender, clear, true).Should().BeFalse();
    }

    [TestMethod]
    public void GetSolidColor_NullBrush_ReturnsEmptyColor()
    {
        Func<Brush?, string, global::System.Drawing.Color> getSolidColor =
            typeof(WinUITextControl).TestAccessor.CreateDelegate<Func<Brush?, string, global::System.Drawing.Color>>(
                "GetSolidColor");

        getSolidColor(null, "BackgroundColor").Should().Be(global::System.Drawing.Color.Empty);
    }

    private static void AssertSameValue<TProjected, TXaml>(TProjected projected, TXaml xaml)
        where TProjected : struct, Enum
        where TXaml : struct, Enum
        => Convert.ToInt64(projected).Should().Be(Convert.ToInt64(xaml));

    private static void AssertProperties(
        Type editorType,
        Type wrapperType,
        IReadOnlyDictionary<string, string> projections)
    {
        string[] declaredProperties = editorType
            .GetProperties(DeclaredPublicInstance)
            .Select(property => property.Name)
            .ToArray();
        projections.Keys.Should().BeEquivalentTo(declaredProperties);

        foreach (KeyValuePair<string, string> projection in projections)
        {
            wrapperType.GetProperty(projection.Value, PublicInstance).Should().NotBeNull(
                $"{editorType.Name}.{projection.Key} should project as {wrapperType.Name}.{projection.Value}");
        }
    }

    private static void AssertEvents(Type editorType, Type wrapperType, IReadOnlyCollection<string> projectedEvents)
    {
        string[] declaredEvents = editorType
            .GetEvents(DeclaredPublicInstance)
            .Select(@event => @event.Name)
            .ToArray();
        projectedEvents.Should().BeEquivalentTo(declaredEvents);

        foreach (string projectedEvent in projectedEvents)
        {
            wrapperType.GetEvent(projectedEvent, PublicInstance).Should().NotBeNull(
                $"{editorType.Name}.{projectedEvent} should be projected by {wrapperType.Name}");
        }
    }

    private static void AddIfXamlTyped(
        ICollection<string> xamlTypedMembers,
        Type wrapperType,
        MemberInfo member,
        Type signatureType)
    {
        if (IsXamlType(signatureType))
        {
            xamlTypedMembers.Add($"{wrapperType.Name}.{member.Name}: {signatureType}");
        }
    }

    private static bool IsXamlType(Type type)
    {
        if (type.HasElementType)
        {
            return IsXamlType(type.GetElementType()!);
        }

        if (type.IsGenericType && type.GetGenericArguments().Any(IsXamlType))
        {
            return true;
        }

        return type.Namespace?.StartsWith("Microsoft.UI.Xaml", StringComparison.Ordinal) is true;
    }
}
