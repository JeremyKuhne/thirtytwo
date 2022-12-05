// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Support;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatch
{
    public int[] GetIDsOfNames(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        if (names.Length == 0)
        {
            return Array.Empty<int>();
        }

        using StringParameterArray namesArg = new(names);
        int[] ids = new int[names.Length];
        fixed (int* i = ids)
        {
            try
            {
                GetIDsOfNames(IID.NULL(), (PWSTR*)(char**)namesArg, (uint)names.Length, lcid: 0, i);
            }
            catch (COMException ex) when (ex.HResult == (int)HRESULT.DISP_E_UNKNOWNNAME)
            {
            }
        }

        return ids;
    }
}