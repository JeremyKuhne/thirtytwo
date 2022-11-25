// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Foundation;

public unsafe partial struct HINSTANCE : IHandle<HINSTANCE>
{
    HINSTANCE IHandle<HINSTANCE>.Handle => this;
    object? IHandle<HINSTANCE>.Wrapper => null;

    /// <inheritdoc cref="FromAddress(void*, bool)"/>
    public static HINSTANCE FromAddress(nint address, bool incrementRefCount = false)
        => FromAddress((void*)address, incrementRefCount);

    /// <summary>
    ///  Gets the module that the specified memory address is in, if any.  Do not release this handle unless
    ///  <paramref name="incrementRefCount"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HINSTANCE*)"/>
    public static HINSTANCE FromAddress(void* address, bool incrementRefCount = false)
    {
        HINSTANCE instance;
        Interop.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS
                | (incrementRefCount ? 0 : Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT),
            (PCWSTR)address,
            &instance);

        return instance;
    }

    /// <summary>
    ///  Gets the module for the launching executable. Do not release this handle.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HINSTANCE*)"/>
    public static HINSTANCE GetLaunchingExecutable()
    {
        HINSTANCE instance;
        Interop.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (PCWSTR)null,
            &instance);

        return instance;
    }

    /// <summary>
    ///  Gets the module for the launching executable. Do not release this handle unless
    ///  <paramref name="incrementRefCount"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HINSTANCE*)"/>
    public static HINSTANCE FromName(string name, bool incrementRefCount = false)
    {
        fixed (char* n = name)
        {
            HINSTANCE instance;

            Interop.GetModuleHandleEx(
                incrementRefCount ? 0 : Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (PCWSTR)n,
                &instance);

            return instance;
        }
    }
}