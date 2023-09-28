// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.System.Registry;

public partial struct HKEY : IDisposable
{
    private const uint REMOTE_HANDLE_TAG = 0x00000001;
    private const uint REG_CLASSES_SPECIAL_TAG = 0x00000002;

    public bool IsPerfKey()
        => this == HKEY_PERFORMANCE_DATA || this == HKEY_PERFORMANCE_NLSTEXT || this == HKEY_PERFORMANCE_TEXT;

    /// <summary>
    ///  Returns true if the key is from the local machine.
    /// </summary>
    public bool IsLocalKey => (Value & REMOTE_HANDLE_TAG) == 0;

    /// <summary>
    ///  Returns true if the key is special (notably in <see cref="HKEY.HKEY_CLASSES_ROOT"/>, where
    ///  it might be redirected to per user settings).
    /// </summary>
    public bool IsSpecialKey => (Value & REG_CLASSES_SPECIAL_TAG) != 0;

    public void Dispose()
    {
        WIN32_ERROR error = Interop.RegCloseKey(this);
        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            error.Throw();
        }
    }
}