// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies rich-text formatting keyboard accelerators that are disabled.</summary>
[Flags]
public enum WinUIRichEditDisabledFormattingAccelerators
{
    /// <summary>Leaves all formatting accelerators enabled.</summary>
    None = 0,

    /// <summary>Disables the bold accelerator.</summary>
    Bold = 1,

    /// <summary>Disables the italic accelerator.</summary>
    Italic = 2,

    /// <summary>Disables the underline accelerator.</summary>
    Underline = 4,

    /// <summary>Disables all formatting accelerators.</summary>
    All = -1
}