// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Ole;

namespace Windows;

/// <summary>Specifies the effect of an OLE drag-and-drop operation.</summary>
[Flags]
public enum DragDropEffects : uint
{
    /// <summary>No data will be dropped.</summary>
    None = DROPEFFECT.DROPEFFECT_NONE,

    /// <summary>The dropped data will be copied.</summary>
    Copy = DROPEFFECT.DROPEFFECT_COPY,

    /// <summary>The dropped data will be moved.</summary>
    Move = DROPEFFECT.DROPEFFECT_MOVE,

    /// <summary>A link to the dropped data will be created.</summary>
    Link = DROPEFFECT.DROPEFFECT_LINK,

    /// <summary>The target is scrolling while the drag remains active.</summary>
    Scroll = DROPEFFECT.DROPEFFECT_SCROLL
}
