// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Dialogs;

public unsafe partial class FileOpenDialog : FileDialog
{
    public FileOpenDialog(IHandle<HWND>? owner = default) : base(CreateInstance(), owner)
    {
    }

    private static IFileDialog* CreateInstance()
    {
        Interop.CoCreateInstance(
            CLSID.FileOpenDialog,
            null,
            CLSCTX.CLSCTX_INPROC_SERVER,
            out IFileOpenDialog* dialog);

        return (IFileDialog*)dialog;
    }

    /// <summary>
    ///  Gets the results. This will throw if called without getting a successful return from
    ///  <see cref="FileDialog.ShowDialog"/>.
    /// </summary>
    public IReadOnlyList<string> GetResults()
    {
        using ComScope<IShellItemArray> items = new(null);
        using ComScope<IFileOpenDialog> dialog = Interface.GetInterface<IFileOpenDialog>();
        dialog.Pointer->GetResults(items).ThrowOnFailure();
        items.Pointer->GetCount(out uint count).ThrowOnFailure();
        string[] paths = new string[(int)count];
        for (int i = 0; i < count; i++)
        {
            using ComScope<IShellItem> item = new(null);
            items.Pointer->GetItemAt(0, item).ThrowOnFailure();
            paths[i] = item.Pointer->GetFullPath();
        }

        return paths;
    }
}