// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using static Windows.Win32.System.Com.VARENUM;

namespace Windows.Win32.System.Com;

public unsafe partial struct VARIANT : IDisposable
{
    public static VARIANT Empty { get; } = default;

    public bool IsEmpty => vt == VT_EMPTY && data.llVal == 0;

    public VARENUM Type => vt & VT_TYPEMASK;

    public bool Byref => vt.HasFlag(VT_BYREF);

    [UnscopedRef]
    public ref VARENUM vt => ref Anonymous.Anonymous.vt;

    [UnscopedRef]
    public ref _Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union data => ref Anonymous.Anonymous.Anonymous;

    public void Dispose() => Clear();

    public void Clear()
    {
        // PropVariantClear is essentially a superset of VariantClear it calls CoTaskMemFree on the following types:
        //
        //     - VT_LPWSTR, VT_LPSTR, VT_CLSID (psvVal)
        //     - VT_BSTR_BLOB (bstrblobVal.pData)
        //     - VT_CF (pclipdata->pClipData, pclipdata)
        //     - VT_BLOB, VT_BLOB_OBJECT (blob.pData)
        //     - VT_STREAM, VT_STREAMED_OBJECT (pStream)
        //     - VT_VERSIONED_STREAM (pVersionedStream->pStream, pVersionedStream)
        //     - VT_STORAGE, VT_STORED_OBJECT (pStorage)
        //
        // If the VARTYPE is a VT_VECTOR, the contents are cleared as above and CoTaskMemFree is also called on
        // cabstr.pElems.
        //
        // https://learn.microsoft.com/windows/win32/api/oleauto/nf-oleauto-variantclear#remarks
        //
        //     - VT_BSTR (SysFreeString)
        //     - VT_DISPATCH / VT_UNKOWN (->Release(), if not VT_BYREF)

        if (IsEmpty)
        {
            return;
        }

        fixed (void* t = &this)
        {
            Interop.PropVariantClear((StructuredStorage.PROPVARIANT*)t);
        }

        vt = VT_EMPTY;
        data = default;
    }

    public static explicit operator decimal(VARIANT value)
    => value.vt == VT_DECIMAL ? value.Anonymous.decVal : ThrowInvalidCast<decimal>();

    public static explicit operator int(VARIANT value)
        => value.vt == VT_I4 || value.vt == VT_INT ? value.data.intVal : ThrowInvalidCast<int>();

    public static explicit operator VARIANT(int value)
        => new()
        {
            vt = VT_I4,
            data = new() { intVal = value }
        };

    public static explicit operator uint(VARIANT value)
        => value.vt == VT_UI4 || value.vt == VT_UINT ? value.data.uintVal : ThrowInvalidCast<uint>();

    public static explicit operator VARIANT(uint value)
        => new()
        {
            vt = VT_UI4,
            data = new() { uintVal = value }
        };

    public static explicit operator bool(VARIANT value)
        => value.vt == VT_BOOL ? value.data.boolVal != VARIANT_BOOL.VARIANT_FALSE : ThrowInvalidCast<bool>();

    public static explicit operator VARIANT(bool value)
        => new()
        {
            vt = VT_BOOL,
            data = new() { boolVal = value ? VARIANT_BOOL.VARIANT_TRUE : VARIANT_BOOL.VARIANT_FALSE }
        };

    public static explicit operator IDispatch*(VARIANT value)
        => value.vt == VT_DISPATCH ? value.data.pdispVal : ThrowInvalidPointerCast<IDispatch>();

    public static explicit operator VARIANT(IDispatch* value)
        => new()
        {
            vt = VT_DISPATCH,
            data = new() { pdispVal = value }
        };

    public static explicit operator VARIANT(BSTR value)
        => new()
        {
            vt = VT_BSTR,
            data = new() { bstrVal = value }
        };

    public static explicit operator string(VARIANT value) => value.vt switch
    {
        VT_BSTR => value.data.bstrVal.ToString(),
        VT_LPWSTR => new((char*)value.data.pcVal.Value),        // Technically a PROPVARIANT.pwszVal
        _ => ThrowInvalidCast<string>(),
    };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowInvalidCast<T>() => throw new InvalidCastException();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T* ThrowInvalidPointerCast<T>() where T : unmanaged => throw new InvalidCastException();
}