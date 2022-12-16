// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Components;
using Windows.Support;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows.Dialogs;

public unsafe partial class FileDialog : ComponentBase, IHandle<HWND>
{
    // https://learn.microsoft.com/windows/win32/shell/common-file-dialog

    protected AgileComPointer<IFileDialog> Interface { get; private set; }
    private HWND _hwnd;
    private readonly uint _cookie;

    public event EventHandler? SelectionChanged;
    public event EventHandler<AcceptEventArgs>? OkClicked;

    public IHandle<HWND>? Owner { get; private set; }

    internal FileDialog(IFileDialog* dialog, IHandle<HWND>? owner = default)
    {
        dialog->Advise(ComHelpers.GetComPointer<IFileDialogEvents>(new FileDialogEvents(this)), out _cookie);

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

    /// <summary>
    ///  The file name in the edit box.
    /// </summary>
    public string FileName
    {
        get
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->GetFileName(out PWSTR pszName);
            string result = new(pszName);
            Interop.CoTaskMemFree(pszName);
            return result;
        }
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->SetFileName(value);
        }
    }

    /// <summary>
    ///  The label for the file name edit box.
    /// </summary>
    public string FileNameLabel
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->SetFileNameLabel(value);
        }
    }

    /// <summary>
    ///  The label of the Open/Save button
    /// </summary>
    public string OkButtonLabel
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->SetOkButtonLabel(value);
        }
    }

    public string DefaultFolder
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = Interop.SHCreateShellItem(value);
            dialog.Value->SetDefaultFolder(item);
        }
    }

    public string InitialFolder
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = Interop.SHCreateShellItem(value);
            dialog.Value->SetFolder(item);
        }
    }

    public string? CurrentSelection
    {
        get
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = new(null);
            HRESULT result = dialog.Value->GetCurrentSelection(item);
            return result.Failed ? null : item.Value->GetFullPath();
        }
    }

    /// <summary>
    ///  Allows associating persisted state with a given <see cref="Guid"/> instead of
    ///  the application overall. Set immediately after dialog creation.
    /// </summary>
    public Guid ClientGuid
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Value->SetClientGuid(value);
        }
    }

    /// <summary>
    ///  Clears persisted state information. See <see cref="ClientGuid"/>.
    /// </summary>
    public void ClearClientData()
    {
        using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
        dialog.Value->ClearClientData();
    }

    public void Close()
    {
        using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
        dialog.Value->Close(WIN32_ERROR.ERROR_CANCELLED.ToHRESULT());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interface.Dispose();
        }
    }
}