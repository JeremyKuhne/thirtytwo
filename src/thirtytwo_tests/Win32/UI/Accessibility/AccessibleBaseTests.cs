// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Accessibility;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace Tests.Windows.Win32.UI.Accessibility;

public unsafe class AccessibleBaseTests
{
    [StaFact]
    public void AccessibleBase_Dispatch()
    {
        AccessibleCallback callback = new();
        using ComScope<IAccessible> accessible = callback.GetComCallableWrapper<IAccessible>();
        IDispatch* dispatch = (IDispatch*)accessible.Value;
        int[] result = dispatch->GetIdsOfNames("accHitTest", "xLeft", "yTop", "pvarChild");
        result.Should().HaveCount(4);
        result[0].Should().Be(Interop.DISPID_ACC_HITTEST);
        result[1].Should().Be(0);
        result[2].Should().Be(1);
        result[3].Should().Be(Interop.DISPID_UNKNOWN);

        result = dispatch->GetIdsOfNames("accChild");
        result.Should().HaveCount(1);

        VARIANT invokeResult = default;
        EXCEPINFO excepinfo = default;
        uint argErr = default;

        VARIANT arg = new()
        {
            vt = VARENUM.VT_I4,
            data = new() { intVal = 3 }
        };

        DISPPARAMS dispparams = new()
        {
            cArgs = 1,
            cNamedArgs = 0,
            rgdispidNamedArgs = null,
            rgvarg = &arg
        };

        dispatch->Invoke(
            result[0],
            IID.Empty(),
            0u,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispparams,
            &invokeResult,
            &excepinfo,
            &argErr);

        callback.ChildRequested.Should().Be(3);
    }

    private class AccessibleCallback : LegacyAccessibleBase
    {
        public AccessibleCallback() { }

        public int ChildRequested { get; private set; } = -1;

        protected override unsafe IDispatch* GetChild(int childId)
        {
            ChildRequested = childId;
            return null;
        }
    }
}
