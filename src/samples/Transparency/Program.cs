// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Transparency;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Run(new ExcludedClientRegionWindow(Window.DefaultBounds, "TransClient", WindowStyles.OverlappedWindow));
        Application.Run(new LayeredWindow(Window.DefaultBounds, "Layered", WindowStyles.OverlappedWindow));
    }

    /// <summary>
    ///  Creates a window that is halfway transparent using the layered style.
    /// </summary>
    private class LayeredWindow : Window
    {
        public LayeredWindow(
            Rectangle bounds,
            string? text = null,
            WindowStyles style = WindowStyles.Overlapped,
            ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
            Window? parentWindow = null,
            WindowClass? windowClass = null,
            nint parameters = 0,
            HMENU menuHandle = default) : base(
                bounds,
                text,
                style,
                extendedStyle | ExtendedWindowStyles.Layered,
                parentWindow,
                windowClass,
                parameters,
                menuHandle)
        {
            Interop.SetLayeredWindowAttributes(Handle, default, 128, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);
        }
    }

    /// <summary>
    ///  A window with a completely transparent client created by applying a region with the client excluded
    ///  to the window.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   While this works, it disables theming so the non-client area will draw in "9x" style and the rendering
    ///   will not scale with DPI settings.
    ///  </para>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/windows/win32/controls/cookbook-overview#when-visual-styles-are-not-applied"/>
    ///  </para>
    /// </remarks>
    private class ExcludedClientRegionWindow : Window
    {
        private Size _lastSize = new(int.MaxValue, int.MaxValue);

        public ExcludedClientRegionWindow(
            Rectangle bounds,
            string? text = null,
            WindowStyles style = WindowStyles.Overlapped,
            ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
            Window? parentWindow = null,
            WindowClass? windowClass = null,
            nint parameters = 0,
            HMENU menuHandle = default) : base(
                bounds,
                text,
                style,
                extendedStyle,
                parentWindow,
                windowClass,
                parameters,
                menuHandle)
        {
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.WindowPositionChanged:
                    Message.WindowPositionChanged changed = new(lParam);
                    Rectangle bounds = changed.Bounds;
                    Size size = bounds.Size;
                    if (_lastSize == size)
                    {
                        break;
                    }

                    // The size has changed, we need to update the region. Find the offset of the client rectangle to
                    // put it into window relative coordinates so we can exclude it from the window region.
                    _lastSize = size;
                    Rectangle clientRect = window.GetClientRectangle();
                    Point location = clientRect.Location;
                    window.ClientToScreen(ref location);
                    Point offset = new(location.X - bounds.X, location.Y - bounds.Y);
                    clientRect.Offset(offset);

                    {
                        using HRGN windowRegion = HRGN.FromRectangle(new(new(0,0), bounds.Size));
                        using HRGN clientRegion = HRGN.FromRectangle(clientRect);
                        HRGN newRegion = HRGN.CreateEmpty();
                        Interop.CombineRgn(newRegion, windowRegion, clientRegion, RGN_COMBINE_MODE.RGN_DIFF);
                        window.SetWindowRegion(newRegion, redraw: true);
                    }

                    break;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
