// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.System.Registry;

/// <summary>
///  Extensions and lifetime helpers for registry key handles.
/// </summary>
public unsafe partial struct HKEY : IDisposable
{
    private const uint REMOTE_HANDLE_TAG = 0x00000001;
    private const uint REG_CLASSES_SPECIAL_TAG = 0x00000002;

    /// <summary>
    ///  Determines whether this handle is one of the performance data pseudo-keys.
    /// </summary>
    /// <returns><see langword="true"/> when this value is a performance key handle.</returns>
    public bool IsPerfKey()
        => this == HKEY_PERFORMANCE_DATA || this == HKEY_PERFORMANCE_NLSTEXT || this == HKEY_PERFORMANCE_TEXT;

    /// <summary>
    ///  Returns true if the key is from the local machine.
    /// </summary>
    public bool IsLocalKey => ((nuint)Value & REMOTE_HANDLE_TAG) == 0;

    /// <summary>
    ///  Returns true if the key is special (notably in <see cref="HKEY.HKEY_CLASSES_ROOT"/>, where
    ///  it might be redirected to per user settings).
    /// </summary>
    public bool IsSpecialKey => ((nuint)Value & REG_CLASSES_SPECIAL_TAG) != 0;

    /// <summary>
    ///  Closes this registry key handle.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Call this for owned handles returned by open/create APIs when they are no longer needed.
    ///  </para>
    /// </remarks>
    /// <exception cref="Exception">Thrown when the underlying <c>RegCloseKey</c> call fails.</exception>
    public void Dispose()
    {
        WIN32_ERROR error = PInvoke.RegCloseKey(this);
        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            error.ThrowThirtyTwoException();
        }
    }
}