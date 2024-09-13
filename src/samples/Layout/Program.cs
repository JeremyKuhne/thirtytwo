// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Messages;
using Windows.Win32.Graphics.Gdi;
using Microsoft.UI;
#pragma warning disable IDE0005 // Using directive is unnecessary.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
#pragma warning restore IDE0005 // Using directive is unnecessary.

using Xaml = Microsoft.UI.Xaml;
using Windows.Win32.Foundation;
using Windows.Graphics;

namespace LayoutSample;

public static class GraphicsExtensions
{
    public static RectInt32 ToRectInt32(this Rectangle rectangle)
        => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
}

public class MyApp : Xaml.Application
{

}

internal class Program
{
    private static DispatcherQueueController? s_dispatcher;

    [STAThread]
    private static void Main()
    {
        // The runtime should have called CoInitializeEx and RoInitialize for STA (see threads.cpp)
        // This is effectively winrt::init_apartment() in C++/WinRT.

        // Returns false as it is already initialized for the thread.
        // HRESULT hr = Windows.Win32.Interop.RoInitialize(Windows.Win32.System.WinRT.RO_INIT_TYPE.RO_INIT_SINGLETHREADED);
        s_dispatcher = DispatcherQueueController.CreateOnCurrentThread();
        WindowsXamlManager.InitializeForCurrentThread();
        var current = Xaml.Application.Current;

        Windows.Application.Run(new LayoutWindow("Layout Demo"));
    }

    public class ColorPicker : XamlControl
    {
        private readonly Xaml.Controls.ColorPicker _colorPicker;

        public ColorPicker(Windows.Window parentWindow) : base(new Xaml.Controls.ColorPicker(), parentWindow)
        {
            _colorPicker = (Xaml.Controls.ColorPicker)_control;
        }

        public override void Layout(Rectangle bounds)
        {
            base.Layout(bounds);
        }
    }

    public class XamlControl : DisposableBase, ILayoutHandler
    {
        //private Panel _panel;
        protected Xaml.Controls.Control _control;

        public XamlControl(Xaml.Controls.Control control, Windows.Window parentWindow)
        {
            XamlSource = new();
            _control = control;
            XamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(parentWindow.Handle));
            //Page page = new();
            //Grid grid = new();
            //grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            //grid.Children.Add(control);
            //page.Content = grid;
            XamlSource.Content = control;

            //_panel = grid;
        }

        protected DesktopWindowXamlSource XamlSource { get; }

        public HWND Handle => (HWND)Win32Interop.GetWindowFromWindowId(XamlSource.SiteBridge.WindowId);

        public virtual void Layout(Rectangle bounds)
        {
            XamlSource.SiteBridge.MoveAndResize(bounds.ToRectInt32());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                XamlSource.Dispose();
            }
        }
    }

    private class LayoutWindow : MainWindow
    {
        private readonly ReplaceableLayout _replaceableLayout;

        private readonly EditControl _editControl;
        private readonly ButtonControl _buttonControl;
        private readonly StaticControl _staticControl;
        private readonly TextLabelControl _textLabel;
        private readonly ColorPicker _colorPicker;
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

            _colorPicker = new ColorPicker(this);

            _textLabel.SetFont("Segoe Print", 20);

            var font = _buttonControl.GetFontHandle();
            _staticControl.SetWindowText($"{font.GetFaceName()} {font.GetQuality()}");

            _replaceableLayout = new ReplaceableLayout(_textLabel);

            this.AddLayoutHandler(Windows.Layout.Vertical(
                (.5f, Windows.Layout.Margin((5, 5, 0, 0), Windows.Layout.Fill(_colorPicker))),
                (.5f, Windows.Layout.Horizontal(
                    (.7f, Windows.Layout.FixedPercent(.4f, _replaceableLayout)),
                    (.3f, Windows.Layout.FixedPercent(.5f, _buttonControl))))));

            MouseHandler handler = new(_buttonControl);
            handler.MouseUp += Handler_MouseUp;
        }

        private void Handler_MouseUp(Windows.Window window, Point position, MouseButton button, MouseKey mouseState)
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
