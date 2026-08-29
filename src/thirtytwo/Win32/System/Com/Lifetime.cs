// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Lifetime management helper for a COM callable wrapper. It holds the created <typeparamref name="TObject"/>
///  wrapper with he given <typeparamref name="TVTable"/>.
/// </summary>
/// <remarks>
///  <para>
///   This should not be created directly. Instead use <see cref="Lifetime{TVTable, TObject}.Allocate"/>.
///  </para>
///  <para>
///   A COM object's memory layout is a virtual function table (vtable) pointer followed by instance data. We're
///   effectively manually creating a COM object here that contains instance data of a GCHandle to the related
///   managed object and a ref count.
///  </para>
/// </remarks>
public unsafe struct Lifetime<TVTable, TObject> where TVTable : unmanaged
{
    /// <summary>
    ///  Pointer to the unmanaged vtable for this COM wrapper instance.
    /// </summary>
    public TVTable* VTable;

    /// <summary>
    ///  Stored <see cref="GCHandle"/> encoded as an <see cref="IUnknown"/> pointer-sized value.
    /// </summary>
    public IUnknown* Handle;

    /// <summary>
    ///  Native COM reference count.
    /// </summary>
    public uint RefCount;

    /// <summary>
    ///  Increments the COM reference count for the wrapper instance.
    /// </summary>
    /// <param name="this">Pointer to the wrapper instance.</param>
    /// <returns>The updated reference count.</returns>
    public static uint AddRef(IUnknown* @this)
        => Interlocked.Increment(ref ((Lifetime<TVTable, TObject>*)@this)->RefCount);

    /// <summary>
    ///  Decrements the COM reference count and frees resources when it reaches zero.
    /// </summary>
    /// <param name="this">Pointer to the wrapper instance.</param>
    /// <returns>The updated reference count.</returns>
    /// <remarks>
    ///  <para>
    ///   When the count reaches zero, the rooted managed object handle is released and native memory allocated for
    ///   the wrapper is freed with <c>CoTaskMemFree</c>.
    ///  </para>
    /// </remarks>
    public static uint Release(IUnknown* @this)
    {
        var lifetime = (Lifetime<TVTable, TObject>*)@this;
        Debug.Assert(lifetime->RefCount > 0);
        uint count = Interlocked.Decrement(ref lifetime->RefCount);
        if (count == 0)
        {
            GCHandle.FromIntPtr((nint)lifetime->Handle).Free();
            PInvoke.CoTaskMemFree(lifetime);
        }

        return count;
    }

    /// <summary>
    ///  Allocate a lifetime wrapper for the given <paramref name="object"/> with the given
    ///  <paramref name="vtable"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This creates a <see cref="GCHandle"/> to root the <paramref name="object"/> until ref
    ///   counting has gone to zero.
    ///  </para>
    ///  <para>
    ///   The <paramref name="vtable"/> should be fixed, typically as a static. Com calls always
    ///   include the "this" pointer as the first argument.
    ///  </para>
    /// </remarks>
    /// <param name="object">The managed object to root for the COM wrapper lifetime.</param>
    /// <param name="vtable">Pointer to the unmanaged vtable for this COM wrapper shape.</param>
    /// <returns>A pointer to the allocated wrapper instance.</returns>
    /// <exception cref="OutOfMemoryException">The native wrapper allocation failed.</exception>
    public static Lifetime<TVTable, TObject>* Allocate(TObject @object, TVTable* vtable)
    {
        GCHandle handle = GCHandle.Alloc(@object);

        // Manually allocate a native instance of this struct.
        var wrapper = (Lifetime<TVTable, TObject>*)PInvoke.CoTaskMemAlloc((nuint)sizeof(Lifetime<TVTable, TObject>));
        if (wrapper is null)
        {
            handle.Free();
            throw new OutOfMemoryException();
        }

        // Assign a pointer to the vtable, store the GCHandle for the related object, and set the initial ref count.
        wrapper->VTable = vtable;
        wrapper->Handle = (IUnknown*)GCHandle.ToIntPtr(handle);
        wrapper->RefCount = 1;

        return wrapper;
    }

    /// <summary>
    ///  Gets the object wrapped by a lifetime wrapper.
    /// </summary>
    /// <param name="this">Pointer to the wrapper instance.</param>
    /// <returns>The managed object associated with the wrapper, if available.</returns>
    public static TObject? GetObject(IUnknown* @this)
    {
        var lifetime = (Lifetime<TVTable, TObject>*)@this;
        return (TObject?)GCHandle.FromIntPtr((nint)lifetime->Handle).Target;
    }
}