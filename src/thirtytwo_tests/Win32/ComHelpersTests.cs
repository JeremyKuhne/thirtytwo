// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Tests.Windows.Win32;

public unsafe class ComHelpersTests
{
    private static readonly Guid s_mediaPlayerClassId = new("6BF52A52-394A-11d3-B153-00C04F79FAA6");

    [StaFact]
    public void CreateComClass_MediaPlayer()
    {
        using ComScope<IUnknown> mediaPlayer = new(ComHelpers.CreateComClass(s_mediaPlayerClassId));

        using ComScope<IDispatch> dispatch = mediaPlayer.TryQueryInterface<IDispatch>(out HRESULT hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IDispatchEx> dispatchEx = mediaPlayer.TryQueryInterface<IDispatchEx>(out hr);
        hr.Succeeded.Should().BeFalse();
    }
}
