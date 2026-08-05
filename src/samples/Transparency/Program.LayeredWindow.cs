// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Transparency;

internal partial class Program
{
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
            PInvoke.SetLayeredWindowAttributes(Handle, default, 128, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);
        }
    }
}