// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Power;
using Windows.Win32.System.Threading;

namespace Windows;

/// <summary>
///  Encapsulates system power requests for presentation mode.
/// </summary>
public sealed class PresentationPowerRequest : DisposableBase.Finalizable
{
    private const string RequestReason = "thirtytwo PresentationPowerRequest";

    private HANDLE _handle;

    /// <summary>
    ///  Gets whether the presentation power request is active.
    /// </summary>
    public bool IsActive => !_handle.IsNull;

    private unsafe PresentationPowerRequest()
    {
        fixed (char* reason = RequestReason)
        {
            REASON_CONTEXT context = new()
            {
                Version = Interop.POWER_REQUEST_CONTEXT_VERSION,
                Flags = POWER_REQUEST_CONTEXT_FLAGS.POWER_REQUEST_CONTEXT_SIMPLE_STRING
            };

            context.Reason.SimpleReasonString = reason;
            _handle = Interop.PowerCreateRequest(&context);
        }

        if (_handle == PInvoke.INVALID_HANDLE_VALUE)
        {
            _handle = default;
            Error.ThrowLastError();
        }

        if (!Interop.PowerSetRequest(_handle, POWER_REQUEST_TYPE.PowerRequestDisplayRequired)
            || !Interop.PowerSetRequest(_handle, POWER_REQUEST_TYPE.PowerRequestSystemRequired))
        {
            WIN32_ERROR error = Error.GetLastError();
            Dispose(disposing: true);
            error.Throw();
        }
    }

    /// <summary>
    ///  Prevents the display from turning off and the system from entering sleep due to user inactivity.
    ///  Disposing the request will clear the power state request.
    /// </summary>
    /// <exception cref="Win32Exception">The request could not be activated.</exception>
    /// <returns>The active presentation power request.</returns>
    public static PresentationPowerRequest Request() => new();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!IsActive)
        {
            return;
        }

        if (!Interop.PowerClearRequest(_handle, POWER_REQUEST_TYPE.PowerRequestSystemRequired) && disposing)
        {
            Debug.Fail($"Failed to clear system power request. {Error.GetLastError()}");
        }

        if (!Interop.PowerClearRequest(_handle, POWER_REQUEST_TYPE.PowerRequestDisplayRequired) && disposing)
        {
            Debug.Fail($"Failed to clear display power request. {Error.GetLastError()}");
        }

        if (!Interop.CloseHandle(_handle) && disposing)
        {
            Debug.Fail($"Failed to close handle. {Error.GetLastError()}");
        }

        _handle = default;
    }
}