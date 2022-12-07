// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Dialogs;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using static Windows.Win32.System.Com.Com;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ComTests
{
    [Fact]
    public void Com_GetComPointer_SameUnknownInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        IUnknown* unknown1 = GetComPointer<IUnknown>(events);
        IUnknown* unknown2 = GetComPointer<IUnknown>(events);

        Assert.True(unknown1 == unknown2);
    }

    [Fact]
    public void Com_GetComPointer_SameInterfaceInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        IFileDialogEvents* iEvents1 = GetComPointer<IFileDialogEvents>(events);
        IFileDialogEvents* iEvents2 = GetComPointer<IFileDialogEvents>(events);

        Assert.True(iEvents1 == iEvents2);
    }
}
