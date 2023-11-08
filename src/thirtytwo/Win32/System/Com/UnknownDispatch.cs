// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base <see cref="IDispatch"/> class for <see cref="IUnknown"/>.
/// </summary>
public unsafe abstract class UnknownDispatch : StandardDispatch<IUnknown>
{
    // StdOle32.tlb
    private static readonly Guid s_stdole = new("00020430-0000-0000-C000-000000000046");

    // We don't release the ITypeInfo to avoid unloading and reloading the standard OLE ITypeLib.
    private static ITypeInfo* TypeInfo { get; } = ComHelpers.GetRegisteredTypeInfo(s_stdole, 2, 0, IUnknown.IID_Guid);

    public UnknownDispatch() : base(TypeInfo) { }
}