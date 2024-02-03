// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Untyped equivalent of <see cref="ComScope{T}"/>. Prefer <see cref="ComScope{T}"/>.
/// </summary>
public readonly unsafe ref struct ComScope
{
    // Keeping internal as nint allows us to use Unsafe methods to get significantly better generated code.
    private readonly nint _value;
    public void* Value => (void*)_value;

    public ComScope(void* value) => _value = (nint)value;

    public static implicit operator void*(in ComScope scope) => (void*)scope._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void**(in ComScope scope) => (void**)Unsafe.AsPointer(ref Unsafe.AsRef(in scope._value));

    public bool IsNull => _value == 0;

    public void Dispose()
    {
        IUnknown* unknown = (IUnknown*)_value;

        // Really want this to be null after disposal to avoid double releases, but we also want
        // to maintain the readonly state of the struct to allow passing as `in` without creating implicit
        // copies (which would break the T** and void** operators).
        *(void**)this = null;
        if (unknown is not null)
        {
            unknown->Release();
        }
    }
}