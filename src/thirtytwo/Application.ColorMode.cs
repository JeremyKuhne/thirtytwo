// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.Accessibility;

namespace Windows;

public static unsafe partial class Application
{
    private static readonly Lock s_colorModeLock = new();
    private static ApplicationColorMode s_colorMode;
    private static ApplicationColorState? s_colorState;
    private static bool s_colorStateInitialized;
    private static bool s_useUndocumentedDarkModeApis = true;
    private static int s_colorGeneration;

    /// <summary>Gets or sets how the application chooses its light or dark color palette.</summary>
    /// <remarks>
    ///  <para>
    ///   The default is <see cref="ApplicationColorMode.System"/>. Changing this property updates existing managed
    ///   windows on their owning UI threads and applies to subsequently created windows.
    ///  </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    public static ApplicationColorMode ColorMode
    {
        get
        {
            lock (s_colorModeLock)
            {
                return s_colorMode;
            }
        }
        set
        {
            if (value is not (ApplicationColorMode.System or ApplicationColorMode.Dark or ApplicationColorMode.Light))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown application color mode.");
            }

            bool changed;
            lock (s_colorModeLock)
            {
                EnsureColorStateLocked();
                if (s_colorMode == value)
                {
                    return;
                }

                s_colorMode = value;
                changed = RefreshColorStateLocked(forceGeneration: true);
            }

            if (changed)
            {
                Window.ApplyApplicationColorModeToWindows();
            }
        }
    }

    /// <summary>Gets or sets whether undocumented Windows dark mode APIs are used for native controls.</summary>
    /// <remarks>
    ///  <para>
    ///   The default is <see langword="true"/>. These APIs are not part of the supported Windows SDK contract and
    ///   can change between Windows releases. Set this property to <see langword="false"/> to use only documented
    ///   color and theme APIs.
    ///  </para>
    /// </remarks>
    public static bool UseUndocumentedDarkModeApis
    {
        get
        {
            lock (s_colorModeLock)
            {
                return s_useUndocumentedDarkModeApis;
            }
        }
        set
        {
            lock (s_colorModeLock)
            {
                EnsureColorStateLocked();
                if (s_useUndocumentedDarkModeApis == value)
                {
                    return;
                }

                s_useUndocumentedDarkModeApis = value;
                _ = RefreshColorStateLocked(forceGeneration: true);
            }

            Window.ApplyApplicationColorModeToWindows();
        }
    }

    /// <summary>Gets a snapshot of the application's resolved color state and semantic palette.</summary>
    /// <remarks>
    ///  <para>
    ///   The snapshot reflects the requested mode, resolved Light or Dark preference, High Contrast precedence,
    ///   native compatibility policy, and current semantic colors. Derived controls can read this property from
    ///   <see cref="Window.OnColorModeChanged"/> to recreate mode-dependent resources.
    ///  </para>
    ///  <para>
    ///   This property can be read before any window is created. Each returned value remains unchanged; a later
    ///   application or system color update produces a new snapshot with a different generation token.
    ///  </para>
    /// </remarks>
    public static ApplicationColorState CurrentColorState
    {
        get
        {
            lock (s_colorModeLock)
            {
                EnsureColorStateLocked();
                return s_colorState!;
            }
        }
    }

    internal static void RefreshSystemColorMode()
    {
        bool changed;
        lock (s_colorModeLock)
        {
            EnsureColorStateLocked();
            changed = RefreshColorStateLocked(forceGeneration: false);
        }

        if (changed)
        {
            Window.ApplyApplicationColorModeToWindows();
        }
    }

    private static void EnsureColorStateLocked()
    {
        if (s_colorStateInitialized)
        {
            return;
        }

        s_colorStateInitialized = true;
        _ = RefreshColorStateLocked(forceGeneration: true);
    }

    private static bool RefreshColorStateLocked(bool forceGeneration)
    {
        bool highContrast = IsHighContrastEnabled();
        bool dark = s_colorMode switch
        {
            ApplicationColorMode.Dark => true,
            ApplicationColorMode.Light => false,
            _ => IsSystemDark()
        };

        ApplicationColorPalette palette = ApplicationColorPalette.Create(dark, highContrast);
        if (!forceGeneration
            && s_colorState is { } state
            && state.IsDark == dark
            && state.IsHighContrast == highContrast
            && state.Palette == palette)
        {
            return false;
        }

        s_colorState = new(
            s_colorMode,
            dark,
            highContrast,
            s_useUndocumentedDarkModeApis,
            UndocumentedDarkMode.IsSupported,
            ++s_colorGeneration,
            palette);
        return true;
    }

    private static bool IsSystemDark()
    {
        return SystemColorModeProvider.TryGetIsDark(out bool dark) && dark;
    }

    private static bool IsHighContrastEnabled()
    {
        HIGHCONTRASTW highContrast = new() { cbSize = (uint)sizeof(HIGHCONTRASTW) };
        return PInvoke.SystemParametersInfo(
            SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETHIGHCONTRAST,
            highContrast.cbSize,
            &highContrast,
            default)
            && highContrast.dwFlags.HasFlag(HIGHCONTRASTW_FLAGS.HCF_HIGHCONTRASTON);
    }
}