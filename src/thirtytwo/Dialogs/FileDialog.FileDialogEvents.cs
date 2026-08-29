// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Dialogs;

public unsafe partial class FileDialog
{
    /// <summary>
    ///  Bridges native <c>IFileDialogEvents</c> callbacks to managed <see cref="FileDialog"/> events.
    /// </summary>
    internal class FileDialogEvents : IFileDialogEvents.Interface, IManagedWrapper<IFileDialogEvents>
    {
        private readonly FileDialog _dialog;

        /// <summary>
        ///  Initializes a new events sink for the specified dialog.
        /// </summary>
        /// <param name="dialog">The managed dialog that receives translated events.</param>
        public FileDialogEvents(FileDialog dialog) => _dialog = dialog;

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnFileOk(IFileDialog*)"/>
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

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnSelectionChange(IFileDialog*)"/>
        public HRESULT OnSelectionChange(IFileDialog* pfd)
        {
            _dialog.SelectionChanged?.Invoke(_dialog, EventArgs.Empty);
            return HRESULT.S_OK;
        }

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnFolderChanging(IFileDialog*, IShellItem*)"/>
        public HRESULT OnFolderChanging(IFileDialog* pfd, IShellItem* psiFolder) => HRESULT.S_OK;

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnFolderChange(IFileDialog*)"/>
        public HRESULT OnFolderChange(IFileDialog* pfd) => HRESULT.S_OK;

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnShareViolation(IFileDialog*, IShellItem*, FDE_SHAREVIOLATION_RESPONSE*)"/>
        public HRESULT OnShareViolation(IFileDialog* pfd, IShellItem* psi, FDE_SHAREVIOLATION_RESPONSE* pResponse) => HRESULT.S_OK;

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnTypeChange(IFileDialog*)"/>
        public HRESULT OnTypeChange(IFileDialog* pfd) => throw new NotImplementedException();

        /// <inheritdoc cref="IFileDialogEvents.Interface.OnOverwrite(IFileDialog*, IShellItem*, FDE_OVERWRITE_RESPONSE*)"/>
        public HRESULT OnOverwrite(IFileDialog* pfd, IShellItem* psi, FDE_OVERWRITE_RESPONSE* pResponse) => HRESULT.S_OK;
    }
}
