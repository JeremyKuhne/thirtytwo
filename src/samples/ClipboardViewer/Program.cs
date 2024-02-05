// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Win32.Foundation;

namespace LayoutSample;

internal class Program
{
    [STAThread]
    private static void Main() => Application.Run(new ClipboardViewer("Clipboard Viewer Demo"));

    private class ClipboardViewer : MainWindow
    {
        private readonly EditControl _editControl;
        private readonly TextLabelControl _textLabel;

        public ClipboardViewer(string title) : base(title: title)
        {
            _editControl = new EditControl(
                editStyle: EditControl.Styles.Multiline | EditControl.Styles.Left
                    | EditControl.Styles.AutoHorizontalScroll | EditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.HorizontalScroll
                    | WindowStyles.VerticalScroll | WindowStyles.Border,
                parentWindow: this);

            _editControl.SetFont("Consolas", 14);

            _textLabel = new TextLabelControl(
                text: "Recent clipboard text:",
                parentWindow: this);

            this.AddLayoutHandler(Layout.Horizontal(
                (.1f, Layout.Margin((5, 5, 0, 0), _textLabel)),
                (.9f, Layout.Fill(_editControl))));

            Clipboard.AddClipboardFormatListener(this);
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.ClipboardUpdate:
                    if (Clipboard.IsClipboardTextAvailable())
                    {
                        string? text = Clipboard.GetClipboardText();
                        if (text is not null)
                        {
                            _editControl.Text = text;
                            _editControl.Invalidate();
                        }
                    }

                    return (LRESULT)0;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }

        protected override void Dispose(bool disposing)
        {
            if (!Handle.IsNull)
            {
                Clipboard.RemoveClipboardFormatListener(this);
            }

            base.Dispose(disposing);
            _editControl.Dispose();
            _textLabel.Dispose();
        }
    }
}
