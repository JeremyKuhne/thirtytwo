// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private static readonly Guid s_mediaPlayerClassId = new("6BF52A52-394A-11d3-B153-00C04F79FAA6");
        private readonly ActiveXControl _mediaPlayer;

        public MainWindow(string title) : base(
            DefaultBounds,
            text: title,
            style: WindowStyles.OverlappedWindow)
        {
            _mediaPlayer = new(s_mediaPlayerClassId, DefaultBounds, this);
            this.AddLayoutHandler(Layout.Fill(_mediaPlayer));
        }
    }
}
