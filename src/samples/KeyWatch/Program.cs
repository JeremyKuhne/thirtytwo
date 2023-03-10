// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Windows;
using Windows.Win32.Foundation;

namespace KeyWatch;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Run(new MainWindow("Key Watcher"));
    }

    private class MainWindow : Window
    {
        private readonly EditControl _editControl;

        public MainWindow(string title) : base(
            DefaultBounds,
            text: title,
            style: WindowStyles.OverlappedWindow)
        {
            _editControl = new EditControl(
                DefaultBounds,
                editStyle: EditControl.Styles.Multiline | EditControl.Styles.Left
                    | EditControl.Styles.AutoHorizontalScroll | EditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.HorizontalScroll
                    | WindowStyles.VerticalScroll | WindowStyles.Border,
                parentWindow: this);

            _editControl.SetFont("Arial", 14);

            this.AddLayoutHandler(Layout.Fill(_editControl));
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.KeyUp:
                    Message.KeyUpDown key = new(wParam, lParam);
                    Debug.WriteLine($"KeyUp: {key.Key}");
                    break;
                case MessageType.KeyDown:
                    Debug.WriteLine($"KeyDown");
                    break;
                case MessageType.UnicodeChar:
                    Debug.WriteLine($"UnicodeChar");
                    break;
                case MessageType.Char:
                    Debug.WriteLine($"Char: {(nint)wParam}");
                    break;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
