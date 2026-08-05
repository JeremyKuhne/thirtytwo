// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.System.Com;

namespace ActiveXSample;

internal partial class Program
{
    private class MediaPlayer(Rectangle bounds, Window parentWindow, nint parameters = 0)
        : ActiveXControl(CLSID.WindowsMediaPlayer, bounds, parentWindow, parameters)
    {
        public string? URL
        {
            get => (string?)GetComProperty("URL");
            set => SetComProperty("URL", value);
        }

        public bool StretchToFit
        {
            get => (bool)(GetComProperty("stretchToFit") ?? false);
            set => SetComProperty("stretchToFit", value);
        }
    }
}