// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Original license (from https://github.com/dotnet/winforms):
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Windows.Support;
using RuntimeMarshal = System.Runtime.InteropServices.Marshal;

namespace Windows.Win32.System.Com;

/// <summary>
///  Wraps an <see cref="IClassFactory"/> from a dynamically loaded assembly.
/// </summary>
internal unsafe class ComClassFactory : IDisposable
{
    private readonly HMODULE _module;
    private readonly bool _unloadModule;
    private readonly IClassFactory* _classFactory;

    public Guid ClassId { get; }

    private const string ExportMethodName = "DllGetClassObject";

    public ComClassFactory(
        string filePath,
        Guid classId) : this(HMODULE.LoadModule(filePath), classId)
    {
        _unloadModule = true;
    }

    public ComClassFactory(
        HMODULE module,
        Guid classId)
    {
        _module = module;
        ClassId = classId;

        // Dynamically get the class factory method.

        // HRESULT DllGetClassObject(
        //   [in] REFCLSID rclsid,
        //   [in] REFIID riid,
        //   [out] LPVOID* ppv
        // );

        FARPROC proc = Interop.GetProcAddress(module, ExportMethodName);

        if (proc.IsNull)
        {
            Error.ThrowLastError();
        }

        IClassFactory* classFactory;
        ((delegate* unmanaged<Guid*, Guid*, void**, HRESULT>)proc.Value)(
            &classId, IID.Get<IClassFactory>(),
            (void**)&classFactory).ThrowOnFailure();
        _classFactory = classFactory;
    }

    internal HRESULT CreateInstance(out IUnknown* unknown)
    {
        unknown = default;
        fixed (IUnknown** u = &unknown)
        {
            return _classFactory->CreateInstance(null, IID.Get<IUnknown>(), (void**)u);
        }
    }

    internal HRESULT CreateInstance(out object? unknown)
    {
        HRESULT result = CreateInstance(out IUnknown* punk);
        unknown = punk is null ? null : RuntimeMarshal.GetObjectForIUnknown((nint)punk);
        return result;
    }

    public void Dispose()
    {
        _classFactory->Release();
        if (_unloadModule && !_module.IsNull)
        {
            Interop.FreeLibrary(_module);
        }
    }
}