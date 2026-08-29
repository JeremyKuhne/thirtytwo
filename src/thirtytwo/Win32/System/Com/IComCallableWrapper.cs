// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Raw <c>IUnknown</c>-compatible COM callable wrapper marker interface used by
///  <see cref="ComWrappers"/>-generated objects in this library.
/// </summary>
/// <remarks>
///  <para>
///   This struct represents a native COM interface pointer shape: a pointer to a vtable whose first three slots are
///   <c>QueryInterface</c>, <c>AddRef</c>, and <c>Release</c>.
///  </para>
///  <para>
///   The methods are raw ABI calls and do not add validation beyond what the native entry points provide.
///  </para>
/// </remarks>
/// <inheritdoc cref="IComCallableWrapper.Interface"/>
public unsafe partial struct IComCallableWrapper : IComIID, IVTable<IComCallableWrapper, IComCallableWrapper.Vtbl>
{
    private readonly void** _vtbl;

    /// <summary>
    ///  The interface identifier for <see cref="IComCallableWrapper"/>.
    /// </summary>
    // {73B17DAF-0480-4702-AF7C-AF3BD4715D71}
    public static readonly Guid IID_Guid = new(0x73b17daf, 0x0480, 0x4702, 0xaf, 0x7c, 0xaf, 0x3b, 0xd4, 0x71, 0x5d, 0x71);

    static ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ReadOnlySpan<byte> data = new byte[]
            {
                // 0x73b17daf, 0x0480, 0x4702, 0xaf, 0x7c, 0xaf, 0x3b, 0xd4, 0x71, 0x5d, 0x71);
                0xaf, 0x7d, 0xb1, 0x73, 0x80, 0x04, 0x02, 0x47, 0xaf, 0x7c, 0xaf, 0x3b, 0xd4, 0x71, 0x5d, 0x71
            };

            return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
        }
    }

    /// <summary>
    ///  Calls <c>QueryInterface</c> for the requested interface identifier.
    /// </summary>
    /// <param name="riid">The interface identifier to query.</param>
    /// <param name="ppvObject">
    ///  Receives the requested interface pointer on success; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>The COM <c>HRESULT</c> returned by the underlying vtable call.</returns>
    public HRESULT QueryInterface(in Guid riid, out void* ppvObject)
    {
        fixed (void** ppvObjectLocal = &ppvObject)
        fixed (Guid* riidLocal = &riid)
        {
            return QueryInterface(riidLocal, ppvObjectLocal);
        }
    }

    /// <summary>
    ///  Calls the raw vtable <c>QueryInterface</c> entry.
    /// </summary>
    /// <param name="riid">Pointer to the interface identifier to query.</param>
    /// <param name="ppvObject">Receives the requested interface pointer on success.</param>
    /// <returns>The COM <c>HRESULT</c> returned by the underlying vtable call.</returns>
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        fixed (IComCallableWrapper* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IComCallableWrapper*, Guid*, void**, HRESULT>)_vtbl[0])(pThis, riid, ppvObject);
    }

    /// <summary>
    ///  Calls <c>AddRef</c> on this COM interface pointer.
    /// </summary>
    /// <returns>The updated COM reference count.</returns>
    public uint AddRef()
    {
        fixed (IComCallableWrapper* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IComCallableWrapper*, uint>)_vtbl[1])(pThis);
    }

    /// <summary>
    ///  Calls <c>Release</c> on this COM interface pointer.
    /// </summary>
    /// <returns>The updated COM reference count.</returns>
    public uint Release()
    {
        fixed (IComCallableWrapper* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IComCallableWrapper*, uint>)_vtbl[2])(pThis);
    }

    /// <summary>
    ///  No vtable population is required for this marker wrapper.
    /// </summary>
    /// <param name="vtable">The vtable storage associated with this interface.</param>
    static void IVTable<IComCallableWrapper, Vtbl>.PopulateVTable(Vtbl* vtable) { }
}