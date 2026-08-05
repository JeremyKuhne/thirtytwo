// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;

namespace ActiveXSample;

internal partial class Program
{
    private class SystemMonitor(Rectangle bounds, Window parentWindow, nint parameters = 0)
        : ActiveXControl(s_systemMonitorClassId, bounds, parentWindow, parameters)
    {
        private static readonly Guid s_systemMonitorClassId = new("C4D2D8E0-D1DD-11CE-940F-008029004347");
    }
}