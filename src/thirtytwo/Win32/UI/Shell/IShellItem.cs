// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.Shell;

/// <summary>
///  Partial helper surface for native <c>IShellItem</c> operations.
/// </summary>
public unsafe partial struct IShellItem
{
    /// <summary>
    ///  Gets the desktop-absolute editing path for the shell item.
    /// </summary>
    /// <returns>The full path string returned by <c>IShellItem::GetDisplayName</c>.</returns>
    /// <remarks>
    ///  <para>
    ///   The native call returns an allocated string pointer.
    ///   This method copies the text into a managed <see cref="string"/> and always releases
    ///   the native allocation with <c>CoTaskMemFree</c> before returning.
    ///  </para>
    ///  <para>
    ///   Failure HRESULT values are converted to exceptions via <c>ThrowOnFailure()</c>.
    ///  </para>
    /// </remarks>
    public string GetFullPath()
    {
        GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out PWSTR ppszName).ThrowOnFailure();
        string result = new(ppszName);
        PInvoke.CoTaskMemFree(ppszName);
        return result;
    }
}