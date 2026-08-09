// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Identifies a window relative to another window in the native window hierarchy or z-order.</summary>
public enum WindowRelationship : uint
{
    /// <summary>Gets the highest window of the same type in z-order.</summary>
    First = GET_WINDOW_CMD.GW_HWNDFIRST,

    /// <summary>Gets the lowest window of the same type in z-order.</summary>
    Last = GET_WINDOW_CMD.GW_HWNDLAST,

    /// <summary>Gets the window immediately below the source window in z-order.</summary>
    Next = GET_WINDOW_CMD.GW_HWNDNEXT,

    /// <summary>Gets the window immediately above the source window in z-order.</summary>
    Previous = GET_WINDOW_CMD.GW_HWNDPREV,

    /// <summary>Gets the source window's owner.</summary>
    Owner = GET_WINDOW_CMD.GW_OWNER,

    /// <summary>Gets the source window's highest child in z-order.</summary>
    Child = GET_WINDOW_CMD.GW_CHILD,

    /// <summary>Gets an enabled owned popup, or the source window when none exists.</summary>
    EnabledPopup = GET_WINDOW_CMD.GW_ENABLEDPOPUP
}