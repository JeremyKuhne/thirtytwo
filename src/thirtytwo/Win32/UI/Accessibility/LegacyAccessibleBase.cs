// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

/// <summary>
///  Use as a base class when you only want to provide Active Accessibility <see cref="IAccessible"/> support.
/// </summary>
public unsafe abstract class LegacyAccessibleBase : AccessibleBase, IManagedWrapper<IAccessible, IDispatch>
{
    public LegacyAccessibleBase() : base()
    {
    }
}