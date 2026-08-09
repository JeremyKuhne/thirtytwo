// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.LibraryLoader;

namespace Windows;

/// <summary>Applies private UxTheme dark-mode preferences when explicitly enabled by the application.</summary>
/// <devdoc>
///  <para>
///   These exports are undocumented and can change between Windows releases. On Windows 10 build 17763, ordinal
///   133 is <c>AllowDarkModeForWindow(HWND, bool)</c> and ordinal 135 is <c>AllowDarkModeForApp(bool)</c>. Starting
///   with build 18362, ordinal 135 retains its number but changes to
///   <c>SetPreferredAppMode(PreferredAppMode)</c>. Keep the signatures separated by the build check below.
///  </para>
///  <para>
///   Export lookup failure disables the private path. The containing module remains loaded for the process lifetime
///   because the cached function pointers are valid only while UxTheme is loaded. Documented palette rendering and
///   <c>SetWindowTheme</c> reset behavior remain available when this adapter is unsupported or disabled.
///  </para>
/// </devdoc>
internal static unsafe class UndocumentedDarkMode
{
    private const ushort AllowDarkModeForWindowOrdinal = 133;
    private const ushort SetPreferredAppModeOrdinal = 135;

    private static readonly Lock s_lock = new();

    // Cached exports require UxTheme to remain loaded for the process lifetime.
    private static readonly HMODULE s_uxTheme = LoadUxTheme();
    private static readonly FARPROC s_allowDarkModeForWindow = GetExport(AllowDarkModeForWindowOrdinal);
    private static readonly FARPROC s_setPreferredAppMode = GetExport(SetPreferredAppModeOrdinal);
    private static int s_configuredGeneration = -1;

    /// <summary>Gets whether the required private UxTheme exports are available on this Windows version.</summary>
    /// <value>
    ///  <see langword="true"/> when Windows is build 17763 or later and both required exports resolved;
    ///  otherwise, <see langword="false"/>.
    /// </value>
    internal static bool IsSupported
        => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)
            && !s_allowDarkModeForWindow.IsNull
            && !s_setPreferredAppMode.IsNull;

    /// <summary>Configures the process-wide private preferred application mode for the current color generation.</summary>
    /// <param name="state">The resolved application color state to apply.</param>
    /// <remarks>
    ///  <para>
    ///   This method does nothing when the private exports are unavailable or the generation has already been
    ///   configured. High Contrast and an application opt-out restore the default private application mode.
    ///  </para>
    /// </remarks>
    internal static void ConfigureApplication(ApplicationColorState state)
    {
        if (!IsSupported)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_configuredGeneration == state.Generation)
            {
                return;
            }

            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
            {
                PreferredAppMode mode = !state.UseUndocumentedDarkModeApis || state.IsHighContrast
                    ? PreferredAppMode.Default
                    : state.IsDark
                        ? PreferredAppMode.ForceDark
                        : PreferredAppMode.ForceLight;

                _ = ((delegate* unmanaged[Stdcall]<PreferredAppMode, PreferredAppMode>)(void*)s_setPreferredAppMode.Value)(mode);
            }
            else
            {
                byte allowDark = state.UseUndocumentedDarkModeApis && state.IsDark && !state.IsHighContrast
                    ? (byte)1
                    : (byte)0;

                _ = ((delegate* unmanaged[Stdcall]<byte, byte>)(void*)s_setPreferredAppMode.Value)(allowDark);
            }

            s_configuredGeneration = state.Generation;
        }
    }

    /// <summary>Applies or removes a private dark visual-style association for a window.</summary>
    /// <param name="window">The window whose visual-style association is updated.</param>
    /// <param name="state">The resolved application color state to apply.</param>
    /// <param name="darkSubAppName">
    ///  The private visual-style sub-app name used when dark mode is active, or <see langword="null"/> when no
    ///  sub-app override is required.
    /// </param>
    /// <param name="darkSubIdList">
    ///  The private visual-style sub-ID list used when dark mode is active, or <see langword="null"/> when no sub-ID
    ///  override is required.
    /// </param>
    /// <remarks>
    ///  <para>
    ///   The private per-window opt-in is enabled only when the exports are supported, the application has not opted
    ///   out, Dark mode is effective, and High Contrast is inactive. Otherwise, <c>SetWindowTheme</c> receives null
    ///   theme names to remove the prior association.
    ///  </para>
    /// </remarks>
    internal static void ApplyWindowTheme(
        HWND window,
        ApplicationColorState state,
        string? darkSubAppName,
        string? darkSubIdList)
    {
        ConfigureApplication(state);

        bool useDarkTheme = ShouldUseDarkTheme(state);

        if (!s_allowDarkModeForWindow.IsNull)
        {
            _ = ((delegate* unmanaged[Stdcall]<HWND, byte, byte>)(void*)s_allowDarkModeForWindow.Value)(
                window,
                useDarkTheme ? (byte)1 : (byte)0);
        }

        _ = PInvoke.SetWindowTheme(
            window,
            useDarkTheme ? darkSubAppName : null,
            useDarkTheme ? darkSubIdList : null);
    }

    private static bool ShouldUseDarkTheme(ApplicationColorState state)
        => IsSupported
            && state.UseUndocumentedDarkModeApis
            && state.IsDark
            && !state.IsHighContrast;

    private static FARPROC GetExport(ushort ordinal)
        => s_uxTheme.IsNull
            ? default
            : PInvoke.GetProcAddress(s_uxTheme, new PCSTR((byte*)(nuint)ordinal));

    private static HMODULE LoadUxTheme()
    {
        fixed (char* moduleName = "uxtheme.dll")
        {
            return PInvoke.LoadLibraryEx(
                moduleName,
                default,
                LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_SEARCH_SYSTEM32);
        }
    }
}