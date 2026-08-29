// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.UI.ViewManagement;

/// <summary>
///  Raw Windows Runtime interface used to read application color preferences.
/// </summary>
/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.getcolorvalue">UISettings.GetColorValue</see>
///   for the projected API. The IID and ABI layout are defined by the Windows SDK's
///   <see href="https://github.com/microsoft/win32metadata/blob/5ced95101b7458c83e3bed4e92c369ff27753648/generation/WinSDK/RecompiledIdlHeaders/winrt/windows.ui.viewmanagement.h#L3999-L4029">IUISettings3 declaration</see>.
///  </para>
/// </remarks>
internal unsafe struct IUISettings3 : IComIID
{
    /// <summary>
    ///  Pointer to the interface vtable in Windows Runtime ABI layout.
    /// </summary>
    private readonly void** _vtable;

    /// <summary>
    ///  Gets the interface identifier for <c>IUISettings3</c>.
    /// </summary>
    static ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ReadOnlySpan<byte> data =
            [
                0xe4, 0x1b, 0x02, 0x03,
                0x54, 0x52,
                0x81, 0x47,
                0x81, 0x94, 0x51, 0x68, 0xf7, 0xd0, 0x6d, 0x7b
            ];

            return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
        }
    }

    /// <summary>
    ///  Invokes the raw ABI form of
    ///  <see href="https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.getcolorvalue">UISettings.GetColorValue</see>.
    /// </summary>
    /// <param name="desiredColor">The semantic color slot to query.</param>
    /// <param name="value">Output pointer that receives the color bytes when the call succeeds.</param>
    /// <returns>The native HRESULT returned by the vtable call. The value is not translated by this method.</returns>
    /// <remarks>
    ///  The function pointer is invoked with the <c>Stdcall</c> calling convention and expects
    ///  the current struct address as the first argument.
    /// </remarks>
    internal HRESULT GetColorValue(UISettingsColorType desiredColor, UISettingsColor* value)
    {
        fixed (IUISettings3* settings = &this)
        {
            return ((delegate* unmanaged[Stdcall]<IUISettings3*, UISettingsColorType, UISettingsColor*, HRESULT>)_vtable[6])(
                settings,
                desiredColor,
                value);
        }
    }
}