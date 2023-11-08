// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace Windows.Accessibility;

/// <summary>
///  Base <see cref="IDispatch"/> class for <see cref="IAccessible"/>.
/// </summary>
public unsafe abstract class AccessibleDispatch : StandardDispatch<IAccessible>
{
    // The accessibility TypeLib- lives in oleacc.dll
    private static readonly Guid s_accessibilityTypeLib = new("1ea4dbf0-3c3b-11cf-810c-00aa00389b71");

    // We don't release the ITypeInfo to avoid unloading and reloading the IAccessible ITypeLib.
    private static ITypeInfo* TypeInfo { get; } = ComHelpers.GetRegisteredTypeInfo(s_accessibilityTypeLib, 1, 1, IAccessible.IID_Guid);

    public AccessibleDispatch() : base(TypeInfo) { }
}