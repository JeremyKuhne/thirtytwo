// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.UI.Shell;

namespace Windows.Win32;

[TestClass]
public unsafe class ComHelpersTests
{
    [STATestMethod]
    public void SHCreateShellItem_NonexistentPath_Throws()
    {
        string path = Path.Combine(Path.GetTempPath(), $"thirtytwo-{Guid.NewGuid():N}", "missing");

        Action action = () =>
        {
            using ComScope<IShellItem> shellItem = PInvoke.SHCreateShellItem(path);
        };

        action.Should().Throw<FileNotFoundException>();
    }

    [STATestMethod]
    public void CreateComClass_MediaPlayer()
    {
        using ComScope<IUnknown> mediaPlayer = new(CLSID.WindowsMediaPlayer.CreateComClass());

        using ComScope<IDispatch> dispatch = mediaPlayer.TryQueryInterface<IDispatch>(out HRESULT hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IDispatchEx> dispatchEx = mediaPlayer.TryQueryInterface<IDispatchEx>(out hr);
        hr.Succeeded.Should().BeFalse();
    }
}
