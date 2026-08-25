// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace IntegrationHost;

internal static unsafe class MouseInput
{
    internal static void Move(Point point) => Send([CreateMove(point)]);

    internal static void PressLeftButton()
        => Send([CreateButton(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN)]);

    internal static void ReleaseLeftButton()
        => Send([CreateButton(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP)]);

    private static INPUT CreateMove(Point point)
        => new()
        {
            type = INPUT_TYPE.INPUT_MOUSE,
            mi = new MOUSEINPUT
            {
                dx = NormalizeCoordinate(
                    point.X,
                    PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
                    PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN)),
                dy = NormalizeCoordinate(
                    point.Y,
                    PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
                    PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN)),
                dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE
                    | MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE
                    | MOUSE_EVENT_FLAGS.MOUSEEVENTF_VIRTUALDESK
            }
        };

    private static INPUT CreateButton(MOUSE_EVENT_FLAGS flags)
        => new()
        {
            type = INPUT_TYPE.INPUT_MOUSE,
            mi = new MOUSEINPUT { dwFlags = flags }
        };

    private static int NormalizeCoordinate(int coordinate, int origin, int extent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(extent, 2);
        long offset = Math.Clamp((long)coordinate - origin, 0, extent - 1L);
        return checked((int)((offset * ushort.MaxValue + ((extent - 1L) / 2)) / (extent - 1L)));
    }

    private static void Send(ReadOnlySpan<INPUT> inputs)
    {
        uint inserted = PInvoke.SendInput(inputs, sizeof(INPUT));
        if (inserted != inputs.Length)
        {
            throw new Win32Exception(
                Marshal.GetLastPInvokeError(),
                $"SendInput inserted {inserted} of {inputs.Length} mouse events.");
        }
    }
}