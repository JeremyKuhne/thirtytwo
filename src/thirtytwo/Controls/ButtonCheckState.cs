// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Specifies the check state of a native button control.</summary>
public enum ButtonCheckState : uint
{
    /// <summary>The button is not checked.</summary>
    Unchecked = 0,

    /// <summary>The button is checked.</summary>
    Checked = 1,

    /// <summary>The button is indeterminate.</summary>
    Indeterminate = 2
}