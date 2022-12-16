// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Dialogs;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ComTests
{
    [Fact]
    public void Com_GetComPointer_SameUnknownInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        using var unknown1 = ComScope<IUnknown>.GetComCallableWrapper(events);
        using var unknown2 = ComScope<IUnknown>.GetComCallableWrapper(events);

        Assert.True(unknown1.Value == unknown2.Value);
    }

    [Fact]
    public void Com_GetComPointer_SameInterfaceInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        using var iEvents1 = ComScope<IFileDialogEvents>.GetComCallableWrapper(events);
        using var iEvents2 = ComScope<IFileDialogEvents>.GetComCallableWrapper(events);

        Assert.True(iEvents1.Value == iEvents2.Value);
    }
}
