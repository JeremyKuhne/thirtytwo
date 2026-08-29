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

    /// <summary>
    ///  Gets the scoped COM pointer value.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The returned pointer is owned by this scope and is released by <see cref="Dispose"/>.
    ///  </para>
    /// </remarks>
    public void* Value => (void*)_value;

    /// <summary>
    ///  Initializes a new scope that owns the provided AddRef'd COM pointer.
    /// </summary>
    /// <param name="value">Pointer to own. May be <see langword="null"/>.</param>
    public ComScope(void* value) => _value = (nint)value;

    /// <summary>
    ///  Converts this scope to a raw pointer value.
    /// </summary>
    /// <param name="scope">The scope to convert.</param>
    /// <returns>The current scoped pointer value.</returns>
    public static implicit operator void*(in ComScope scope) => (void*)scope._value;

    /// <summary>
    ///  Converts this scope to a writable pointer-to-pointer destination.
    /// </summary>
    /// <param name="scope">The scope whose storage receives a COM output pointer.</param>
    /// <returns>A <c>void**</c> that points to this scope's internal storage.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void**(in ComScope scope) => (void**)Unsafe.AsPointer(ref Unsafe.AsRef(in scope._value));

    /// <summary>
    ///  Gets whether the scoped pointer is <see langword="null"/>.
    /// </summary>
    public bool IsNull => _value == 0;

    /// <summary>
    ///  Releases the owned COM reference, if any, and clears the scoped pointer.
    /// </summary>
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