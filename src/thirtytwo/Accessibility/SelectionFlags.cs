// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/selflag">SELFLAG Constants</see>
///   documentation.
///  </para>
/// </remarks>
[Flags]
public enum SelectionFlags : int
{
    TakeFocus = (int)Interop.SELFLAG_TAKEFOCUS,
    TakeSelection = (int)Interop.SELFLAG_TAKESELECTION,
    ExtendSelection = (int)Interop.SELFLAG_EXTENDSELECTION,
    AddSelection = (int)Interop.SELFLAG_ADDSELECTION,
    RemoveSelection = (int)Interop.SELFLAG_REMOVESELECTION,
}