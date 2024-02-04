// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public class GdiPlusException : ThirtyTwoException
{
    public GdiPlusException(Status status) : base(status.ToString()) => Status = status;
    public Status Status { get; private set; }
}
