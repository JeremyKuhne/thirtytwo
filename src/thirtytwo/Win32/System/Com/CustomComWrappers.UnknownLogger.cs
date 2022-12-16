// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// #define TRACK_UNKNOWN
#if TRACK_UNKNOWN

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

internal sealed unsafe partial class CustomComWrappers
{
    internal static IUnknown.Vtbl* LoggerVTable { get; } = Initialize();

    private static IUnknown.Vtbl* Initialize()
    {
        IUnknown.Vtbl* vtable = (IUnknown.Vtbl*)RuntimeHelpers.AllocateTypeAssociatedMemory(
            typeof(CustomComWrappers),
            sizeof(IUnknown.Vtbl));

        vtable->QueryInterface_1 = &QueryInterface;
        vtable->AddRef_2 = &AddRef;
        vtable->Release_3 = &Release;

        return vtable;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT QueryInterface(IUnknown* @this, Guid* riid, void** ppvObject)
    {
        HRESULT hr = s_wrappersUnknown->QueryInterface_1(@this, riid, ppvObject);
        Debug.WriteLineIf(
            hr.Failed,
            $"QueryInterface failed for interface {{{(riid is null ? "null" : riid->ToString())}}} for instance {(nint)@this:X8}");
        return hr;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint AddRef(IUnknown* @this)
    {
        uint count = s_wrappersUnknown->AddRef_2(@this);
        return count;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint Release(IUnknown* @this)
    {
        uint count = s_wrappersUnknown->Release_3(@this);
        return count;
    }
}

#endif