// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using NativeWindow = Windows.Window;

namespace IntegrationHost;

internal sealed class TextDragSourceEditControl : EditControl
{
    internal TextDragSourceEditControl(Rectangle bounds, NativeWindow parent)
        : base(
            bounds,
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.Border,
            editStyle: Styles.Left | Styles.AutoHorizontalScroll | Styles.NoHideSelection,
            parentWindow: parent)
    {
    }

    internal event EventHandler? SourceTextChanged;

    protected override void OnCommand(int controlId, int notificationCode)
    {
        base.OnCommand(controlId, notificationCode);
        if (notificationCode == PInvoke.EN_CHANGE)
        {
            SourceTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}