// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

public unsafe partial struct VARIANT : IDisposable
{
    public static VARIANT Empty { get; } = default;

    public bool IsEmpty => vt == VARENUM.VT_EMPTY && data.llVal == 0;

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

        vt = VARENUM.VT_EMPTY;
        data = default;
    }

    public static explicit operator int(VARIANT value)
        => value.vt == VARENUM.VT_I4 || value.vt == VARENUM.VT_INT ? value.data.intVal : ThrowInvalidCast<int>();

    public static explicit operator VARIANT(int value)
        => new()
        {
            vt = VARENUM.VT_I4,
            data = new() { intVal = value }
        };

    public static explicit operator bool(VARIANT value)
        => value.vt == VARENUM.VT_BOOL ? value.data.boolVal != VARIANT_BOOL.VARIANT_FALSE : ThrowInvalidCast<bool>();

    public static explicit operator VARIANT(bool value)
        => new()
        {
            vt = VARENUM.VT_BOOL,
            data = new() { boolVal = value ? VARIANT_BOOL.VARIANT_TRUE : VARIANT_BOOL.VARIANT_FALSE }
        };

    public static explicit operator IDispatch*(VARIANT value)
        => value.vt == VARENUM.VT_DISPATCH ? value.data.pdispVal : ThrowInvalidPointerCast<IDispatch>();

    public static explicit operator VARIANT(IDispatch* value)
        => new()
        {
            vt = VARENUM.VT_DISPATCH,
            data = new() { pdispVal = value }
        };

    public static explicit operator VARIANT(BSTR value)
        => new()
        {
            vt = VARENUM.VT_BSTR,
            data = new() { bstrVal = value }
        };

    public static explicit operator string(VARIANT value) => value.vt switch
    {
        VARENUM.VT_BSTR => value.data.bstrVal.ToString(),
        VARENUM.VT_LPWSTR => new((char*)value.data.pcVal.Value),        // Technically a PROPVARIANT.pwszVal
        _ => ThrowInvalidCast<string>(),
    };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowInvalidCast<T>() => throw new InvalidCastException();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T* ThrowInvalidPointerCast<T>() where T : unmanaged => throw new InvalidCastException();
}