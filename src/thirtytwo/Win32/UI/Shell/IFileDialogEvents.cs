// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Shell;

public unsafe partial struct IFileDialogEvents : IVTable<IFileDialogEvents, IFileDialogEvents.Vtbl>
{
    static void IVTable<IFileDialogEvents, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->OnFileOk_4 = &OnFileOk;
        vtable->OnFolderChanging_5 = &OnFolderChanging;
        vtable->OnFolderChange_6 = &OnFolderChange;
        vtable->OnSelectionChange_7 = &OnSelectionChange;
        vtable->OnShareViolation_8 = &OnShareViolation;
        vtable->OnTypeChange_9 = &OnTypeChange;
        vtable->OnOverwrite_10 = &OnOverwrite;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnFileOk(IFileDialogEvents* @this, IFileDialog* pfd)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnFileOk(pfd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnFolderChanging(IFileDialogEvents* @this, IFileDialog* pfd, IShellItem* psiFolder)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnFolderChanging(pfd, psiFolder));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnFolderChange(IFileDialogEvents* @this, IFileDialog* pfd)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnFolderChange(pfd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnSelectionChange(IFileDialogEvents* @this, IFileDialog* pfd)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnSelectionChange(pfd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnShareViolation(IFileDialogEvents* @this, IFileDialog* pfd, IShellItem* psi, FDE_SHAREVIOLATION_RESPONSE* pResponse)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnShareViolation(pfd, psi, pResponse));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnTypeChange(IFileDialogEvents* @this, IFileDialog* pfd)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnTypeChange(pfd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnOverwrite(IFileDialogEvents* @this, IFileDialog* pfd, IShellItem* psi, FDE_OVERWRITE_RESPONSE* pResponse)
        => Com.UnwrapAndInvoke<IFileDialogEvents, Interface>(@this, o => o.OnOverwrite(pfd, psi, pResponse));
}