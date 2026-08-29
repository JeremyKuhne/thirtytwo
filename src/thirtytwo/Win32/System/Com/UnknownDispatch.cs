// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base <see cref="IDispatch"/> class for <see cref="IUnknown"/>.
/// </summary>
public unsafe abstract class UnknownDispatch : StandardDispatch<IUnknown>
{
    /// <summary>
    ///  Type library identifier for StdOle32.
    /// </summary>
    // StdOle32.tlb
    private static readonly Guid s_stdole = new("00020430-0000-0000-C000-000000000046");

    /// <summary>
    ///  Cached <see cref="ITypeInfo"/> pointer for <see cref="IUnknown"/> dispatch metadata.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This pointer is intentionally kept for process lifetime to avoid repeatedly loading and unloading the
    ///   standard OLE type library.
    ///  </para>
    /// </remarks>
    // We don't release the ITypeInfo to avoid unloading and reloading the standard OLE ITypeLib.
    private static ITypeInfo* TypeInfo { get; } = s_stdole.GetRegisteredTypeInfo(2, 0, IUnknown.IID_Guid);

    /// <summary>
    ///  Initializes a new dispatch object backed by the cached <see cref="IUnknown"/> type information.
    /// </summary>
    public UnknownDispatch() : base(TypeInfo) { }
}