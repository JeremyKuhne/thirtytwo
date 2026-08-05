// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace ActiveXSample;

internal partial class Program
{
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
}