// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

public partial struct POINTS
{
    /// <summary>
    ///  Converts a <see cref="POINTS"/> value to a managed <see cref="Point"/>.
    /// </summary>
    /// <param name="point">The source coordinates.</param>
    /// <returns>A managed point with matching coordinate values.</returns>
    public static implicit operator Point(POINTS point) => new(point.x, point.y);
}