// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows.Dialogs;

/// <summary>
///  Wraps the Windows common file dialog COM object and exposes managed events and options.
/// </summary>
public unsafe partial class FileDialog : ComponentBase, IHandle<HWND>
{
    // https://learn.microsoft.com/windows/win32/shell/common-file-dialog

    /// <summary>
    ///  Gets the agile COM pointer that owns the underlying <c>IFileDialog</c> instance.
    /// </summary>
    protected AgileComPointer<IFileDialog> Interface { get; private set; }
    private HWND _hwnd;
    private readonly uint _cookie;

    /// <summary>
    ///  Occurs when the selected item changes in the dialog.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Occurs when the user activates the Open or Save button.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Handlers can set <see cref="AcceptEventArgs.Accept"/> to <see langword="false"/> to block dialog
    ///   acceptance.
    ///  </para>
    /// </remarks>
    public event EventHandler<AcceptEventArgs>? OkClicked;

    /// <summary>
    ///  Gets the owner window handle wrapper used when showing the dialog.
    /// </summary>
    public IHandle<HWND>? Owner { get; private set; }

    /// <summary>
    ///  Initializes a new <see cref="FileDialog"/> wrapper around an existing <c>IFileDialog</c> pointer.
    /// </summary>
    /// <param name="dialog">The native file dialog interface pointer to own.</param>
    /// <param name="owner">The optional owner window for modal display.</param>
    internal FileDialog(IFileDialog* dialog, IHandle<HWND>? owner = default)
    {
        using ComScope<IFileDialogEvents> events = new(
            new FileDialogEvents(this).GetComPointer<IFileDialogEvents>());
        _ = dialog->Advise(events.Pointer, out _cookie);

        // Wrap in an agile reference so it will be safely finalized if Dispose isn't called.
        Interface = new AgileComPointer<IFileDialog>(dialog, takeOwnership: true);
        Owner = owner;
    }

    /// <summary>
    ///  Gets the native window handle for the current dialog instance.
    /// </summary>
    public HWND Handle
    {
        get
        {
            if (_hwnd.IsNull)
            {
                using var scope = Interface.GetInterface<IOleWindow>();
                scope.Pointer->GetWindow(out _hwnd);
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
        HRESULT result = fileDialog.Pointer->Show(Owner?.Handle ?? default);
        return result.Succeeded
            || (result == WIN32_ERROR.ERROR_CANCELLED.ToHRESULT() ? false : throw result);
    }

    /// <summary>
    ///  Gets or sets the option flags for the underlying common file dialog.
    /// </summary>
    public Options DialogOptions
    {
        get
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Pointer->GetOptions(out var options);
            return (Options)options;
        }
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Pointer->SetOptions((FILEOPENDIALOGOPTIONS)value);
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
            dialog.Pointer->GetFileName(out PWSTR pszName);
            string result = new(pszName);
            PInvoke.CoTaskMemFree(pszName);
            return result;
        }
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            dialog.Pointer->SetFileName(value);
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
            dialog.Pointer->SetFileNameLabel(value);
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
            dialog.Pointer->SetOkButtonLabel(value);
        }
    }

    /// <summary>
    ///  Sets the default folder used when the dialog has no persisted last-visited location.
    /// </summary>
    public string DefaultFolder
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = PInvoke.SHCreateShellItem(value);
            dialog.Pointer->SetDefaultFolder(item);
        }
    }

    /// <summary>
    ///  Sets the initial folder shown when the dialog opens.
    /// </summary>
    public string InitialFolder
    {
        set
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = PInvoke.SHCreateShellItem(value);
            dialog.Pointer->SetFolder(item);
        }
    }

    /// <summary>
    ///  Gets the currently selected item path, if the dialog can provide one.
    /// </summary>
    /// <returns>The current selection path; otherwise <see langword="null"/> if no selection is available.</returns>
    public string? CurrentSelection
    {
        get
        {
            using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
            using ComScope<IShellItem> item = new(null);
            HRESULT result = dialog.Pointer->GetCurrentSelection(item);
            return result.Failed ? null : item.Pointer->GetFullPath();
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
            dialog.Pointer->SetClientGuid(value);
        }
    }

    object? IHandle<HWND>.Wrapper => this;

    /// <summary>
    ///  Clears persisted state information. See <see cref="ClientGuid"/>.
    /// </summary>
    public void ClearClientData()
    {
        using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
        dialog.Pointer->ClearClientData();
    }

    /// <summary>
    ///  Closes the dialog by signaling an explicit user-cancel result.
    /// </summary>
    public void Close()
    {
        using ComScope<IFileDialog> dialog = Interface.GetInterface<IFileDialog>();
        dialog.Pointer->Close(WIN32_ERROR.ERROR_CANCELLED.ToHRESULT());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (_cookie != 0)
                {
                    using ComScope<IFileDialog> dialog = Interface.TryGetInterface(out HRESULT result);
                    if (result.Succeeded)
                    {
                        _ = dialog.Pointer->Unadvise(_cookie);
                    }
                }
            }
            finally
            {
                Interface.Dispose();
            }
        }
    }
}