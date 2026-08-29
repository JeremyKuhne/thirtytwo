// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Dialogs;

/// <summary>
///  Displays the shell file-open picker and exposes selected results.
/// </summary>
public sealed unsafe partial class FileOpenDialog : FileDialog
{
    /// <summary>
    ///  Initializes a file-open dialog wrapper.
    /// </summary>
    /// <param name="owner">The optional owner window used for modal display.</param>
    public FileOpenDialog(IHandle<HWND>? owner = default) : base(CreateInstance(), owner)
    {
    }

    /// <summary>
    ///  Creates the native <c>IFileOpenDialog</c> COM instance.
    /// </summary>
    /// <returns>A pointer to the created <c>IFileDialog</c> interface.</returns>
    private static IFileDialog* CreateInstance()
    {
        PInvoke.CoCreateInstance(
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
    /// <returns>The full file-system paths selected by the user.</returns>
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
            items.Pointer->GetItemAt((uint)i, item).ThrowOnFailure();
            paths[i] = item.Pointer->GetFullPath();
        }

        return paths;
    }
}