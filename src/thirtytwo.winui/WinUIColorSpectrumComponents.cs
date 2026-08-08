// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies how HSV components map to a WinUI color picker's spectrum axes.</summary>
public enum WinUIColorSpectrumComponents
{
    /// <summary>Maps hue to the horizontal axis and saturation to the vertical axis.</summary>
    HueSaturation,

    /// <summary>Maps hue to the horizontal axis and value to the vertical axis.</summary>
    HueValue,

    /// <summary>Maps saturation to the horizontal axis and hue to the vertical axis.</summary>
    SaturationHue,

    /// <summary>Maps saturation to the horizontal axis and value to the vertical axis.</summary>
    SaturationValue,

    /// <summary>Maps value to the horizontal axis and hue to the vertical axis.</summary>
    ValueHue,

    /// <summary>Maps value to the horizontal axis and saturation to the vertical axis.</summary>
    ValueSaturation
}