// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace Windows.Win32.System.Ole;

public unsafe class LoadRegTypeLibTests
{
    private static readonly Guid s_accessibilityTypeLib = new("1ea4dbf0-3c3b-11cf-810c-00aa00389b71");

    [StaFact]
    public void LoadRegTypeLib_Accessibility()
    {
        using ComScope<ITypeLib> typelib = new(null);
        Interop.LoadRegTypeLib(s_accessibilityTypeLib, 1, 1, 0, typelib);
        typelib.IsNull.Should().BeFalse();

        using ComScope<ITypeInfo> typeinfo = new(null);
        typelib.Pointer->GetTypeInfoOfGuid(IID.Get<IAccessible>(), typeinfo);
        typeinfo.IsNull.Should().BeFalse();
    }
}
