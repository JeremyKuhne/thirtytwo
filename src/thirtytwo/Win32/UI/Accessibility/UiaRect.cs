// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

public partial struct UiaRect
{
    public static explicit operator UiaRect(Rectangle value) => new()
    {
        height = value.Height,
        width = value.Width,
        left = value.X,
        top = value.Y
    };

    public static explicit operator Rectangle(UiaRect value) => new(
        (int)Math.Round(value.left),
        (int)Math.Round(value.top),
        (int)Math.Round(value.width),
        (int)Math.Round(value.height));

    public unsafe VARIANT ToVARIANT()
    {
        VARIANT variant = default;
        fixed (UiaRect* u = &this)
        {
            Interop.InitVariantFromDoubleArray((double*)u, 4, &variant).ThrowOnFailure();
        }

        return variant;
    }
}