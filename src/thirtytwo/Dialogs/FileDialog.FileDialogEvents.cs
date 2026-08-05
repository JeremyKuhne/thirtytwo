// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Dialogs;

public unsafe partial class FileDialog
{
    internal class FileDialogEvents : IFileDialogEvents.Interface, IManagedWrapper<IFileDialogEvents>
    {
        private readonly FileDialog _dialog;

        public FileDialogEvents(FileDialog dialog) => _dialog = dialog;

        public HRESULT OnFileOk(IFileDialog* pfd)
        {
            if (_dialog.OkClicked is { } clicked)
            {
                AcceptEventArgs args = new();
                clicked.Invoke(_dialog, args);
                return args.Accept ? HRESULT.S_OK : PInvoke.S_FALSE;
            }

            return HRESULT.S_OK;
        }

        public HRESULT OnSelectionChange(IFileDialog* pfd)
        {
            _dialog.SelectionChanged?.Invoke(_dialog, EventArgs.Empty);
            return HRESULT.S_OK;
        }

        public HRESULT OnFolderChanging(IFileDialog* pfd, IShellItem* psiFolder) => HRESULT.S_OK;
        public HRESULT OnFolderChange(IFileDialog* pfd) => HRESULT.S_OK;
        public HRESULT OnShareViolation(IFileDialog* pfd, IShellItem* psi, FDE_SHAREVIOLATION_RESPONSE* pResponse) => HRESULT.S_OK;
        public HRESULT OnTypeChange(IFileDialog* pfd) => throw new NotImplementedException();
        public HRESULT OnOverwrite(IFileDialog* pfd, IShellItem* psi, FDE_OVERWRITE_RESPONSE* pResponse) => HRESULT.S_OK;
    }
}