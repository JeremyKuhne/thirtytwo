// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.UI.WindowsAndMessaging;

internal readonly ref struct ThreadWindowEnumerator
{
    private readonly GCHandle _callback;

    public unsafe ThreadWindowEnumerator(uint threadId, Func<HWND, bool> callback)
    {
        _callback = GCHandle.Alloc(callback, GCHandleType.Normal);
        Interop.EnumThreadWindows(threadId, &CallBack, (nint)_callback);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static BOOL CallBack(HWND hwnd, LPARAM lParam)
    {
        var callback = (Func<HWND, bool>)(GCHandle.FromIntPtr(lParam).Target!);
        return callback(hwnd);
    }

    public void Dispose()
    {
        if (_callback.IsAllocated)
        {
            _callback.Free();
        }
    }
}