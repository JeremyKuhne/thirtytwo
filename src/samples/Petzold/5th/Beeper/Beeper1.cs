// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Beeper;

internal class Beeper1 : MainWindow
{
    private bool _flipFlop = false;
    private nuint _timerId;

    public Beeper1(string title) : base(title) { }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                _timerId = window.SetTimer(1000);
                return (LRESULT)0;
            case MessageType.Timer:
                Interop.MessageBeep(MESSAGEBOX_STYLE.MB_OK);
                _flipFlop = !_flipFlop;
                window.Invalidate();
                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    using HBRUSH brush = HBRUSH.CreateSolid(_flipFlop ? Color.Red : Color.Blue);
                    dc.FillRectangle(window.GetClientRectangle(), brush);
                }
                return (LRESULT)0;
            case MessageType.Destroy:
                window.KillTimer(_timerId);
                break;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
