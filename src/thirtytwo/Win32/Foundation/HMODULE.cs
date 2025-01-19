// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.LibraryLoader;

namespace Windows.Win32.Foundation;

public unsafe partial struct HMODULE : IHandle<HMODULE>
{
    private const string DllGetVersionMethodName = "DllGetVersion";

    HMODULE IHandle<HMODULE>.Handle => this;
    object? IHandle<HMODULE>.Wrapper => null;

    /// <inheritdoc cref="FromAddress(void*, bool)"/>
    public static HMODULE FromAddress(nint address, bool incrementRefCount = false)
        => FromAddress((void*)address, incrementRefCount);

    /// <summary>
    ///  Gets the module that the specified memory address is in, if any.  Do not release this handle unless
    ///  <paramref name="incrementRefCount"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HMODULE*)"/>
    public static HMODULE FromAddress(void* address, bool incrementRefCount = false)
    {
        HMODULE hmodule;
        Interop.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS
                | (incrementRefCount ? 0 : Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT),
            (PCWSTR)address,
            &hmodule);

        return hmodule;
    }

    /// <summary>
    ///  Gets the module for the launching executable. Do not release this handle.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HMODULE*)"/>
    public static HMODULE GetLaunchingExecutable()
    {
        HMODULE hmodule;
        Interop.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (PCWSTR)null,
            &hmodule);

        return hmodule;
    }

    /// <summary>
    ///  Gets the module for the launching executable. Do not release this handle unless
    ///  <paramref name="incrementRefCount"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The found instance or <see cref="Null"/>.</returns>
    /// <inheritdoc cref="Interop.GetModuleHandleEx(uint, PCWSTR, HMODULE*)"/>
    public static HMODULE FromName(string name, bool incrementRefCount = false)
    {
        fixed (char* n = name)
        {
            HMODULE hmodule;

            Interop.GetModuleHandleEx(
                incrementRefCount ? 0 : Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (PCWSTR)n,
                &hmodule);

            return hmodule;
        }
    }

    /// <summary>
    ///  Loads the module from the specified file path. Throws on failure.
    /// </summary>
    public static HMODULE LoadModule(string filePath, LOAD_LIBRARY_FLAGS flags = default)
    {
        HMODULE module = Interop.LoadLibraryEx(filePath, flags);
        if (module.IsNull)
        {
            Error.ThrowLastError();
        }

        return module;
    }

    /// <summary>
    ///  Gets the dll version of the module. Throws if the module doesn't expose `DllGetVersion`.
    /// </summary>
    public Version GetDllVersion()
    {
        FARPROC proc = Interop.GetProcAddress(this, DllGetVersionMethodName);

        if (proc.IsNull)
        {
            Error.ThrowLastError();
        }

        DLLVERSIONINFO versionInfo = new()
        {
            cbSize = (uint)sizeof(DLLVERSIONINFO)
        };

        // HRESULT Dllgetversionproc(DLLVERSIONINFO* version)
        ((delegate* unmanaged<DLLVERSIONINFO*, HRESULT>)proc.Value)(&versionInfo).ThrowOnFailure();

        return new Version(
            (int)versionInfo.dwMajorVersion,
            (int)versionInfo.dwMinorVersion,
            (int)versionInfo.dwBuildNumber,
            (int)versionInfo.dwPlatformID);
    }

    /// <summary>
    ///  Gets the address of the specified procedure. Throws if the procedure can't be found.
    /// </summary>
    public FARPROC GetProcAddress(string procName)
    {
        FARPROC proc = Interop.GetProcAddress(this, procName);
        if (proc.IsNull)
        {
            Error.ThrowLastError();
        }

        return proc;
    }
}