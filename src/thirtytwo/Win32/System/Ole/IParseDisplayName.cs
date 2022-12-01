// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IParseDisplayName : IVTable<IParseDisplayName, IParseDisplayName.Vtbl>
{
    static void IVTable<IParseDisplayName, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->ParseDisplayName_4 = &ParseDisplayName;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ParseDisplayName(IParseDisplayName* @this, IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
        => UnwrapAndInvoke<IParseDisplayName, Interface>(@this, o => o.ParseDisplayName(pbc, pszDisplayName, pchEaten, ppmkOut));
}
