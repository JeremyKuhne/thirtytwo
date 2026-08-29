// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <summary>
///  Specifies selection operations for <c>IAccessible.accSelect</c>.
/// </summary>
/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/selflag">SELFLAG Constants</see>
///   documentation.
///  </para>
/// </remarks>
[Flags]
public enum SelectionFlags : int
{
    /// <summary>
    ///  Moves keyboard focus to the target object.
    /// </summary>
    TakeFocus = (int)Interop.SELFLAG_TAKEFOCUS,

    /// <summary>
    ///  Clears existing selection and selects the target object.
    /// </summary>
    TakeSelection = (int)Interop.SELFLAG_TAKESELECTION,

    /// <summary>
    ///  Extends the current selection to include the target object.
    /// </summary>
    ExtendSelection = (int)Interop.SELFLAG_EXTENDSELECTION,

    /// <summary>
    ///  Adds the target object to the current selection.
    /// </summary>
    AddSelection = (int)Interop.SELFLAG_ADDSELECTION,

    /// <summary>
    ///  Removes the target object from the current selection.
    /// </summary>
    RemoveSelection = (int)Interop.SELFLAG_REMOVESELECTION,
}