// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.Shell;

public unsafe partial struct IShellItem
{
    public string GetFullPath()
    {
        GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out PWSTR ppszName);
        string result = new(ppszName);
        Interop.CoTaskMemFree(ppszName);
        return result;
    }
}