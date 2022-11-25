// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows.Dialogs;

public unsafe partial class FileDialog : ComponentBase, IHandle<HWND>
{
    // https://learn.microsoft.com/windows/win32/shell/common-file-dialog

    protected AgileComPointer<IFileDialog> Interface { get; private set; }
    private HWND _hwnd;

    public event EventHandler? SelectionChanged;
    public event EventHandler<AcceptEventArgs>? OkClicked;

    public IHandle<HWND>? Owner { get; private set; }

    internal FileDialog(IFileDialog* dialog, IHandle<HWND>? owner = default)
    {
        // Wrap in an agile reference so it will be safely finalized if Dispose isn't called.
        Interface = new AgileComPointer<IFileDialog>(dialog);
        Owner = owner;
    }

    public HWND Handle
    {
        get
        {
            if (_hwnd.IsNull)
            {
                using var scope = Interface.GetInterface<IOleWindow>();
                scope.Value->GetWindow(out _hwnd);
            }

            return _hwnd;
        }
    }

    /// <summary>
    ///  Shows the dialog.
    /// </summary>
    /// <returns><see langword="true"/> if successful, <see langword="false"/> if cancelled.</returns>
    public bool ShowDialog()
    {
        using var modalScope = Application.EnterThreadModalScope();
        using var fileDialog = Interface.GetInterface();
        HRESULT result = fileDialog.Value->Show(Owner?.Handle ?? default);
        return result.Succeeded || (result == WIN32_ERROR.ERROR_CANCELLED.ToHRESULT() ? false : throw result);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interface.Dispose();
        }
    }

    public Options DialogOptions
    {
        get
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->GetOptions(out var options);
            return (Options)options;
        }
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->SetOptions((FILEOPENDIALOGOPTIONS)value);
        }
    }
}