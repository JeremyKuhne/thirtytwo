// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Messages;
using Windows.Win32.Graphics.Gdi;

namespace LayoutSample;

internal class Program
{
    [STAThread]
    private static void Main() => Application.Run(new LayoutWindow("Layout Demo"));

    private class LayoutWindow : MainWindow
    {
        private readonly ReplaceableLayout _replaceableLayout;

        private readonly EditControl _editControl;
        private readonly ButtonControl _buttonControl;
        private readonly StaticControl _staticControl;
        private readonly TextLabelControl _textLabel;
        private readonly HBRUSH _blueBrush;

        public LayoutWindow(string title) : base(title: title)
        {
            _editControl = new EditControl(
                text: "Type text here...",
                editStyle: EditControl.Styles.Multiline | EditControl.Styles.Left
                    | EditControl.Styles.AutoHorizontalScroll | EditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.HorizontalScroll
                    | WindowStyles.VerticalScroll | WindowStyles.Border,
                parentWindow: this);

            _editControl.SetFont("Times New Roman", 24);

            _buttonControl = new ButtonControl(
                text: "Push Me",
                parentWindow: this);

            _staticControl = new StaticControl(
                text: "You pushed it!",
                parentWindow: this);

            _blueBrush = HBRUSH.CreateSolid(Color.Blue);

            _textLabel = new TextLabelControl(
                text: "Text Label Control",
                parentWindow: this,
                textColor: Color.White,
                backgroundColor: Color.Blue,
                features: Features.EnableDirect2d
                // features: default
                );

            _textLabel.SetFont("Segoe Print", 20);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editControl.Dispose();
                _buttonControl.Dispose();
                _staticControl.Dispose();
                _textLabel.Dispose();
                _blueBrush.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
