// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.ApplicationInstallationAndServicing;

namespace Windows.Support;

internal unsafe class ActivationContext
{
    private readonly HANDLE _activationContext;

    public unsafe nuint Activate()
    {
        nuint cookie;
        return Interop.ActivateActCtx(_activationContext, &cookie) ? cookie : 0;
    }

    public static unsafe void Deactivate(nuint cookie)
    {
        Interop.DeactivateActCtx(0, cookie);
    }

    public ActivationContext(HINSTANCE module, int nativeResourceManifestID)
    {
        ACTCTXW actctxw = new()
        {
            cbSize = (uint)sizeof(ACTCTXW),
            lpResourceName = (char*)nativeResourceManifestID,
            dwFlags = Interop.ACTCTX_FLAG_HMODULE_VALID | Interop.ACTCTX_FLAG_RESOURCE_NAME_VALID,
            hModule = module
        };

        _activationContext = Interop.CreateActCtx(&actctxw);

        if (_activationContext == HANDLE.INVALID_HANDLE_VALUE)
        {
            Error.ThrowLastError();
        }
    }
}