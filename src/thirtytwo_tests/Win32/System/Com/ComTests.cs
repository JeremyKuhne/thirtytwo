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
        using ComScope<IUnknown> unknown1 = events.GetComCallableWrapper();
        using ComScope<IUnknown> unknown2 = events.GetComCallableWrapper();

        Assert.True(unknown1.Value == unknown2.Value);
    }

    [Fact]
    public void Com_GetComPointer_SameInterfaceInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        using ComScope<IFileDialogEvents> iEvents1 = events.GetComCallableWrapper<IFileDialogEvents>();
        using ComScope<IFileDialogEvents> iEvents2 = events.GetComCallableWrapper<IFileDialogEvents>();

        Assert.True(iEvents1.Value == iEvents2.Value);
    }
}
