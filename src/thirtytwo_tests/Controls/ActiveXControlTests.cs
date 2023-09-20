// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Win32.System.Com;

namespace Tests.Windows.Controls;

public class ActiveXControlTests
{
    [StaFact]
    public void ActiveXControl_MediaPlayer_Create()
    {
        using Window container = new(Window.DefaultBounds);
        using ActiveXControl control = new(CLSID.WindowsMediaPlayer, Window.DefaultBounds, container);
        container.AddLayoutHandler(Layout.Fill(control));
    }
}
