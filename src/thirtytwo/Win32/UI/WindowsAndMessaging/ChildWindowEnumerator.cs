// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
///  Enumerates child windows for a parent <see cref="HWND"/> using a managed callback.
/// </summary>
internal readonly ref struct ChildWindowEnumerator
{
    /// <summary>
    ///  GC handle used to keep the managed callback alive while native enumeration runs.
    /// </summary>
    private readonly GCHandle _callback;

    /// <summary>
    ///  Starts child window enumeration for the specified parent window.
    /// </summary>
    /// <param name="parent">Parent window whose child windows are enumerated.</param>
    /// <param name="callback">
    ///  Managed callback invoked for each enumerated child window. Returning <see langword="false"/>
    ///  stops enumeration; returning <see langword="true"/> continues.
    /// </param>
    /// <remarks>
    ///  The delegate is rooted with <see cref="GCHandle"/> and passed through <see cref="LPARAM"/>
    ///  to the native callback thunk.
    /// </remarks>
    public unsafe ChildWindowEnumerator(HWND parent, Func<HWND, bool> callback)
    {
        _callback = GCHandle.Alloc(callback, GCHandleType.Normal);
        PInvoke.EnumChildWindows(parent, &CallBack, (nint)_callback);
    }

    /// <summary>
    ///  Native callback thunk for <c>EnumChildWindows</c>.
    /// </summary>
    /// <param name="hwnd">Current child window.</param>
    /// <param name="lParam">Opaque value containing the <see cref="GCHandle"/> for the delegate.</param>
    /// <returns><see langword="true"/> to continue enumeration; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    ///  The method is exported with <c>Stdcall</c> ABI via <see cref="UnmanagedCallersOnlyAttribute"/>.
    /// </remarks>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static BOOL CallBack(HWND hwnd, LPARAM lParam)
    {
        var callback = (Func<HWND, bool>)(GCHandle.FromIntPtr(lParam).Target!);
        return callback(hwnd);
    }

    /// <summary>
    ///  Releases the callback GC handle if it was allocated.
    /// </summary>
    public void Dispose()
    {
        if (_callback.IsAllocated)
        {
            _callback.Free();
        }
    }
}