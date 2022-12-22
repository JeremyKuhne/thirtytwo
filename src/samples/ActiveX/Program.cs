// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;

namespace ActiveXSample;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Run(new MainWindow("ActiveX Demo"));
    }

    private class MainWindow : Window
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly MediaPlayer _mediaPlayer2;

        public MainWindow(string title) : base(
            DefaultBounds,
            text: title,
            style: WindowStyles.OverlappedWindow)
        {
            _mediaPlayer = new(DefaultBounds, this)
            {
                URL = Path.GetFullPath("Media.mpg"),
                StretchToFit = true
            };


            _mediaPlayer2 = new(DefaultBounds, this)
            {
                URL = Path.GetFullPath("Media.mpg"),
            };

            this.AddLayoutHandler(Layout.Vertical(
                (.5f, _mediaPlayer),
                (.5f, _mediaPlayer2)));
        }
    }

    private class MediaPlayer : ActiveXControl
    {
        private static readonly Guid s_mediaPlayerClassId = new("6BF52A52-394A-11d3-B153-00C04F79FAA6");

        public MediaPlayer(Rectangle bounds, Window parentWindow, nint parameters = 0)
            : base(s_mediaPlayerClassId, bounds, parentWindow, parameters)
        {
        }

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

    private class SystemMonitor : ActiveXControl
    {
        private static readonly Guid s_systemMonitorClassId = new("C4D2D8E0-D1DD-11CE-940F-008029004347");

        public SystemMonitor(Rectangle bounds, Window parentWindow, nint parameters = 0)
            : base(s_systemMonitorClassId, bounds, parentWindow, parameters)
        {
        }
    }
}