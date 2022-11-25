// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Messages;

namespace LayoutSample;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Run(new MainWindow("Layout Demo"));
    }

    private class MainWindow : Window
    {
        private readonly ReplaceableLayout _replaceableLayout;

        private readonly EditControl _editControl;
        private readonly ButtonControl _buttonControl;
        private readonly StaticControl _staticControl;
        private readonly TextLabelControl _textLabel;

        public MainWindow(string title) : base(
            DefaultBounds,
            text: title,
            style: WindowStyles.OverlappedWindow)
        {
            _editControl = new EditControl(
                DefaultBounds,
                "Type text here...",
                editStyle: EditControl.Styles.Multiline | EditControl.Styles.Left
                    | EditControl.Styles.AutoHorizontalScroll | EditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.HorizontalScroll
                    | WindowStyles.VerticalScroll | WindowStyles.Border,
                parentWindow: this);

            _editControl.SetFont("Times New Roman", 24);

            _buttonControl = new ButtonControl(
                DefaultBounds,
                text: "Push Me",
                style: WindowStyles.Child | WindowStyles.Visible,
                parentWindow: this);

            _staticControl = new StaticControl(
                DefaultBounds,
                text: "You pushed it!",
                style: WindowStyles.Child | WindowStyles.Visible,
                parentWindow: this);

            _textLabel = new TextLabelControl(
                DefaultBounds,
                text: "Text Label Control",
                style: WindowStyles.Child | WindowStyles.Visible,
                parentWindow: this);

            var font = _buttonControl.GetFontHandle();
            _staticControl.SetWindowText($"{font.GetFaceName()} {font.GetQuality()}");

            _replaceableLayout = new ReplaceableLayout(_textLabel);

            this.AddLayoutHandler(Layout.Vertical(
                (.5f, Layout.Margin((5, 5, 0, 0), Layout.Fill(_editControl))),
                (.5f, Layout.Horizontal(
                    (.7f, Layout.FixedPercent(.4f, _replaceableLayout)),
                    (.3f, Layout.FixedPercent(.5f, _buttonControl))))));

            MouseHandler handler = new(_buttonControl);
            handler.MouseUp += Handler_MouseUp;
        }

        private void Handler_MouseUp(Window window, Point position, MouseButton button, MouseKey mouseState)
        {
            if (_replaceableLayout.Handler == _staticControl)
            {
                _staticControl.ShowWindow(ShowWindowCommand.Hide);
                _textLabel.ShowWindow(ShowWindowCommand.Show);
                _replaceableLayout.Handler = _textLabel;
            }
            else
            {
                _replaceableLayout.Handler = _staticControl;
                _textLabel.ShowWindow(ShowWindowCommand.Hide);
                _staticControl.ShowWindow(ShowWindowCommand.Show);
            }
        }
    }
}
