// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies text wrapping behavior.</summary>
public enum WinUITextWrapping
{
    /// <summary>Does not wrap text.</summary>
    NoWrap = 1,

    /// <summary>Wraps lines at available character boundaries.</summary>
    Wrap = 2,

    /// <summary>Wraps whole words where possible.</summary>
    WrapWholeWords = 3
}