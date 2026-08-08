// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.WinRT;
using Windows.Win32.UI.ViewManagement;

namespace Windows;

/// <summary>Reads the supported Windows application color preference through UISettings.</summary>
internal static unsafe class SystemColorModeProvider
{
    // https://learn.microsoft.com/uwp/api/windows.ui.viewmanagement.uisettings
    private const string UISettingsRuntimeClass = "Windows.UI.ViewManagement.UISettings";

    internal static bool TryGetIsDark(out bool dark)
    {
        dark = false;
        if (!TryGetColor(UISettingsColorType.Foreground, out UISettingsColor foreground))
        {
            return false;
        }

        dark = IsLight(foreground);
        return true;
    }

    internal static bool TryGetColor(UISettingsColorType colorType, out UISettingsColor color)
    {
        color = default;
        RO_INIT_TYPE initializationType = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA
            ? RO_INIT_TYPE.RO_INIT_SINGLETHREADED
            : RO_INIT_TYPE.RO_INIT_MULTITHREADED;
        HRESULT initializationResult = PInvoke.RoInitialize(initializationType);
        bool uninitialize = initializationResult.Succeeded;
        if (initializationResult.Failed)
        {
            return false;
        }

        HSTRING runtimeClass = default;
        try
        {
            fixed (char* runtimeClassPointer = UISettingsRuntimeClass)
            {
                HRESULT stringResult = PInvoke.WindowsCreateString(
                    runtimeClassPointer,
                    (uint)UISettingsRuntimeClass.Length,
                    &runtimeClass);

                if (stringResult.Failed)
                {
                    return false;
                }
            }

            using ComScope<IInspectable> inspectable = new(null);
            if (PInvoke.RoActivateInstance(runtimeClass, inspectable).Failed)
            {
                return false;
            }

            using ComScope<IUISettings3> settings = new(((IUnknown*)inspectable.Pointer)->TryQueryInterface<IUISettings3>());
            if (settings.IsNull)
            {
                return false;
            }

            UISettingsColor result = default;
            if (settings.Pointer->GetColorValue(colorType, &result).Failed)
            {
                return false;
            }

            color = result;
            return true;
        }
        finally
        {
            if (!runtimeClass.IsNull)
            {
                _ = PInvoke.WindowsDeleteString(runtimeClass);
            }

            if (uninitialize)
            {
                PInvoke.RoUninitialize();
            }
        }
    }

    /// <summary>Classifies a color with Microsoft's documented weighted-brightness heuristic.</summary>
    /// <remarks>
    ///  <para>
    ///   A light application foreground indicates Dark mode. This is a quick classifier, not a general luminance
    ///   model. See <see href="https://learn.microsoft.com/windows/apps/desktop/modernize/ui/apply-windows-themes#know-when-dark-mode-is-enabled">Support Dark and Light themes in Win32 apps</see>.
    ///  </para>
    /// </remarks>
    internal static bool IsLight(UISettingsColor color)
        => ((5 * color.G) + (2 * color.R) + color.B) > (8 * 128);
}