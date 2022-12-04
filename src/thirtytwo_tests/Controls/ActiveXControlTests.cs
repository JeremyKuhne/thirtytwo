// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace Tests.Windows.Controls;

public class ActiveXControlTests
{
    private static Guid s_mediaPlayerClassId = new("6BF52A52-394A-11d3-B153-00C04F79FAA6");

    [Fact]
    public void ActiveXControl_MediaPlayer_Create()
    {
        using ActiveXControl control = new(s_mediaPlayerClassId, Window.DefaultBounds);
    }
}
