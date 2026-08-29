// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Represents a GDI+ operation failure.
/// </summary>
public class GdiPlusException : ThirtyTwoException
{
    /// <summary>
    ///  Initializes an exception for a failed GDI+ status value.
    /// </summary>
    /// <param name="status">The failed GDI+ status value.</param>
    public GdiPlusException(Status status) : base(status.ToString()) => Status = status;

    /// <summary>
    ///  Gets the failed GDI+ status value.
    /// </summary>
    public Status Status { get; private set; }
}