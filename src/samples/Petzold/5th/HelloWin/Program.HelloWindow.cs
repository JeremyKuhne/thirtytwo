// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio;

namespace HelloWin;

internal unsafe static partial class Program
{
    private class HelloWindow : MainWindow
    {
        public HelloWindow(string title) : base(title: title, backgroundColor: Color.White)
        {
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Create:
                    PInvoke.PlaySound(
                        (char*)SoundAliasSystemHand,
                        HMODULE.Null,
                        SND_FLAGS.SND_ASYNC | SND_FLAGS.SND_NODEFAULT | SND_FLAGS.SND_ALIAS_ID);
                    return (LRESULT)0;
                case MessageType.Paint:
                    using (DeviceContext dc = window.BeginPaint())
                    {
                        dc.DrawText(
                            "Hello, Windows 98!",
                            window.GetClientRectangle(),
                            DrawTextFormat.SingleLine | DrawTextFormat.Center | DrawTextFormat.VerticallyCenter);
                    }
                    return (LRESULT)0;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}