// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

public partial struct POINTS
{
    public static implicit operator Point(POINTS point) => new(point.x, point.y);
}