// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

public unsafe partial struct VARIANT
{
    public static VARIANT Empty { get; } = default;

    [UnscopedRef]
    public ref VARENUM vt => ref Anonymous.Anonymous.vt;

    [UnscopedRef]
    public ref _Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union data => ref Anonymous.Anonymous.Anonymous;

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowInvalidCast<T>() => throw new InvalidCastException();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T* ThrowInvalidPointerCast<T>() where T : unmanaged => throw new InvalidCastException();
}