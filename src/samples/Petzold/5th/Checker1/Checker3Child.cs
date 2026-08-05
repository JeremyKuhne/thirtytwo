// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;

namespace Checker;

internal unsafe class Checker3Child : WindowClass
{
    public Checker3Child() : base(windowExtraBytes: sizeof(void*))
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                window.SetWindowLong(0, 0); // on/off flag
                return (LRESULT)0;
            case MessageType.LeftButtonDown:
                window.SetWindowLong(0, 1 ^ (int)window.GetWindowLong(0));
                window.Invalidate(false);
                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    Rectangle rect = window.GetClientRectangle();
                    dc.Rectangle(rect);

                    if (window.GetWindowLong(0) != 0)
                    {
                        dc.MoveTo(default);
                        dc.LineTo(new Point(rect.Right, rect.Bottom));
                        dc.MoveTo(new Point(0, rect.Bottom));
                        dc.LineTo(new Point(rect.Right, 0));
                    }
                }

                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}