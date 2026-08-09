// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.WinUI;

/// <summary>Hosts a WinUI TextBox and projects its editing contract through .NET types.</summary>
public sealed class WinUITextBox : WinUITextControl
{
    /// <summary>Creates a WinUI TextBox attached to <paramref name="parentWindow"/>.</summary>
    /// <param name="bounds">The control bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The native parent window.</param>
    public WinUITextBox(Rectangle bounds, Window parentWindow)
        : base(bounds, parentWindow, richEdit: false)
    {
    }

    /// <summary>Occurs before a text change is committed.</summary>
    public event EventHandler<WinUITextBoxBeforeTextChangingEventArgs>? BeforeTextChanging;

    /// <summary>Gets or sets the placeholder foreground color.</summary>
    public Color PlaceholderForegroundColor
    {
        get => GetSolidColor(GetTextBox().PlaceholderForeground, nameof(PlaceholderForegroundColor));
        set => GetTextBox().PlaceholderForeground = ToBrush(value);
    }

    /// <summary>Gets the rectangle for a character index in editor-client view pixels.</summary>
    /// <param name="characterIndex">The zero-based character index.</param>
    /// <param name="trailingEdge">Whether to return the trailing rather than leading edge.</param>
    public RectangleF GetRectangleFromCharacterIndex(int characterIndex, bool trailingEdge = false)
    {
        Windows.Foundation.Rect rectangle = GetTextBox().GetRectFromCharacterIndex(characterIndex, trailingEdge);
        return new(
            Convert.ToSingle(rectangle.X),
            Convert.ToSingle(rectangle.Y),
            Convert.ToSingle(rectangle.Width),
            Convert.ToSingle(rectangle.Height));
    }

    private protected override bool OnBeforeTextChanging(string newText)
    {
        WinUITextBoxBeforeTextChangingEventArgs eventArgs = new(newText);
        BeforeTextChanging?.Invoke(this, eventArgs);
        return eventArgs.Cancel;
    }
}
