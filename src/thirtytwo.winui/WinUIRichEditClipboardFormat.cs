// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies formats copied from a rich editor.</summary>
public enum WinUIRichEditClipboardFormat
{
    /// <summary>Copies every supported format.</summary>
    AllFormats = 0,

    /// <summary>Copies plain text only.</summary>
    PlainText = 1
}