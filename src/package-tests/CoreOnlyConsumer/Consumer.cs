// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CoreOnlyConsumer;

public static class Consumer
{
    public static Type WindowType => typeof(Windows.Window);

    public static Windows.ApplicationColorMode ColorMode
    {
        get => Windows.Application.ColorMode;
        set => Windows.Application.ColorMode = value;
    }

    public static Windows.ApplicationColorState ColorState => Windows.Application.CurrentColorState;

    public static Windows.ApplicationColorMode RequestedMode => ColorState.RequestedMode;

    public static bool IsDark => ColorState.IsDark;

    public static bool IsHighContrast => ColorState.IsHighContrast;

    public static bool UseUndocumentedDarkModeApis => ColorState.UseUndocumentedDarkModeApis;

    public static bool UndocumentedDarkModeApisSupported => ColorState.UndocumentedDarkModeApisSupported;

    public static int ColorGeneration => ColorState.Generation;
}