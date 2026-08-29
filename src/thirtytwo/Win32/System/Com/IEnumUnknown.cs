// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  COM <c>IEnumUnknown</c> projection extended with local vtable population support.
/// </summary>
/// <remarks>
///  <para>
///   This partial declaration adds the <see cref="IVTable{TSelf, TVTable}"/> contract used by the COM wrapper
///   infrastructure.
///  </para>
/// </remarks>
public partial struct IEnumUnknown : IVTable<IEnumUnknown, IEnumUnknown.Vtbl>
{
}