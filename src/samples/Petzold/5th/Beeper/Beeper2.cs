// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Beeper;

internal class Beeper2 : MainWindow
{
    private bool _flipFlop = false;
    private nuint _timerId;
    private TimerProcedure? _timerCallback;

    public Beeper2(string title) : base(title) { }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                _timerCallback = TimerProcedure;
                _timerId = window.SetTimer(1000, callback: _timerCallback);
                return (LRESULT)0;
            case MessageType.Destroy:
                window.KillTimer(_timerId);
                break;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    private void TimerProcedure(HWND window, MessageType message, nuint timerId, uint time)
    {
        Interop.MessageBeep(MESSAGEBOX_STYLE.MB_OK);
        _flipFlop = !_flipFlop;
        using DeviceContext dc = window.GetDeviceContext();
        using HBRUSH brush = HBRUSH.CreateSolid(_flipFlop ? Color.Red : Color.Blue);
        dc.FillRectangle(window.GetClientRectangle(), brush);
    }
}
