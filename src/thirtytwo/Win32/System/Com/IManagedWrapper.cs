// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  An interface that provides a COM callable wrapper for the implmenting class.
/// </summary>
internal interface IManagedWrapper
{
    /// <summary>
    ///  Gets the COM interface table.
    /// </summary>
    ComInterfaceTable GetInterfaceTable();
}