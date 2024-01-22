// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Windows;

public unsafe class DeviceContextTests
{
    [Fact]
    public void DeviceContext_ScreenDeviceContextBehavior()
    {
        using DeviceContext context = DeviceContext.Create();
        int logicalDpi = context.GetDeviceCaps(GET_DEVICE_CAPS_INDEX.LOGPIXELSX);

        Point point = new(0, 8192);
        Interop.LPtoDP(context, &point, 1);
        point.Should().Be(new Point(0, (int)(8192 * (logicalDpi / (float)Interop.USER_DEFAULT_SCREEN_DPI))));
    }
}
