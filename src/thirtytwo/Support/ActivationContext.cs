// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.ApplicationInstallationAndServicing;

namespace Windows.Support;

/// <summary>
///  Wraps a Win32 activation context handle and activates or deactivates it through activation cookies.
/// </summary>
internal unsafe class ActivationContext
{
    private readonly HANDLE _activationContext;

    /// <summary>
    ///  <para>
    ///   Activates this context for the current thread and returns the cookie that must be deactivated later.
    ///  </para>
    /// </summary>
    /// <returns>
    ///  <para>
    ///   A non-zero activation cookie on success; otherwise zero when activation fails.
    ///  </para>
    /// </returns>
    public nuint Activate()
    {
        nuint cookie;
        return PInvoke.ActivateActCtx(_activationContext, &cookie) ? cookie : 0;
    }

    /// <summary>
    ///  <para>
    ///   Deactivates an activation context cookie previously returned by Activate.
    ///  </para>
    /// </summary>
    /// <param name="cookie">
    ///  <para>
    ///   The cookie returned by Activate.
    ///  </para>
    /// </param>
    public static void Deactivate(nuint cookie)
    {
        PInvoke.DeactivateActCtx(0, cookie);
    }

    /// <summary>
    ///  <para>
    ///   Creates an activation context from a manifest resource in a loaded module.
    ///  </para>
    /// </summary>
    /// <param name="module">
    ///  <para>
    ///   The module that contains the native manifest resource.
    ///  </para>
    /// </param>
    /// <param name="nativeResourceManifestID">
    ///  <para>
    ///   The integer resource identifier for the manifest.
    ///  </para>
    /// </param>
    /// <remarks>
    ///  <para>
    ///   The native activation context handle is owned by this instance and is used for activation calls.
    ///  </para>
    ///  <para>
    ///   This type does not currently expose explicit release of the native handle.
    ///  </para>
    /// </remarks>
    /// <exception cref="Exception">
    ///  <para>
    ///   Thrown when context creation fails. The exception type is selected by thirtytwo's Win32 error mapping.
    ///  </para>
    /// </exception>
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