// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.ApplicationInstallationAndServicing;

namespace Windows.Support;

internal unsafe class ActivationContext
{
    private readonly HANDLE _activationContext;

    public nuint Activate()
    {
        nuint cookie;
        return PInvoke.ActivateActCtx(_activationContext, &cookie) ? cookie : 0;
    }

    public static void Deactivate(nuint cookie)
    {
        PInvoke.DeactivateActCtx(0, cookie);
    }

    public ActivationContext(HINSTANCE module, int nativeResourceManifestID)
    {
        ACTCTXW actctxw = new()
        {
            cbSize = (uint)sizeof(ACTCTXW),
            lpResourceName = (char*)nativeResourceManifestID,
            dwFlags = PInvoke.ACTCTX_FLAG_HMODULE_VALID | PInvoke.ACTCTX_FLAG_RESOURCE_NAME_VALID,
            hModule = module
        };

        _activationContext = PInvoke.CreateActCtx(&actctxw);

        if (_activationContext == PInvoke.INVALID_HANDLE_VALUE)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }
    }
}