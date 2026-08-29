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
internal unsafe class ComClassFactory : DisposableBase
{
    private readonly HMODULE _module;
    private readonly bool _unloadModule;
    private readonly IClassFactory* _classFactory;

    /// <summary>
    ///  Gets the class identifier resolved by this factory.
    /// </summary>
    public Guid ClassId { get; }

    private const string ExportMethodName = "DllGetClassObject";

    /// <summary>
    ///  Loads a COM server module from <paramref name="filePath"/> and resolves an <see cref="IClassFactory"/>
    ///  for <paramref name="classId"/>.
    /// </summary>
    /// <param name="filePath">Path to a module that exports <c>DllGetClassObject</c>.</param>
    /// <param name="classId">CLSID to request from <c>DllGetClassObject</c>.</param>
    /// <remarks>
    ///  <para>
    ///   The resolved <see cref="IClassFactory"/> pointer is an AddRef'd COM reference owned by this instance and
    ///   released by <see cref="Dispose"/>.
    ///  </para>
    /// </remarks>
    public ComClassFactory(
        string filePath,
        Guid classId) : this(HMODULE.LoadModule(filePath), classId)
    {
        _unloadModule = true;
    }

    /// <summary>
    ///  Resolves an <see cref="IClassFactory"/> for <paramref name="classId"/> from an already loaded module.
    /// </summary>
    /// <param name="module">Loaded module handle that exports <c>DllGetClassObject</c>.</param>
    /// <param name="classId">CLSID to request from <c>DllGetClassObject</c>.</param>
    /// <remarks>
    ///  <para>
    ///   The caller retains ownership of <paramref name="module"/> lifetime unless this instance was created through
    ///   the module-loading <see cref="ComClassFactory"/> constructor.
    ///  </para>
    ///  <para>
    ///   The resolved <see cref="IClassFactory"/> pointer is an AddRef'd COM reference owned by this instance and
    ///   released by <see cref="Dispose"/>.
    ///  </para>
    /// </remarks>
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

        FARPROC proc = PInvoke.GetProcAddress(module, ExportMethodName);

        if (proc.IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        IClassFactory* classFactory;
        ((delegate* unmanaged<Guid*, Guid*, void**, HRESULT>)proc.Value)(
            &classId, IID.Get<IClassFactory>(),
            (void**)&classFactory).ThrowOnFailure();
        _classFactory = classFactory;
    }

    /// <summary>
    ///  Creates a COM instance and returns its <see cref="IUnknown"/> pointer.
    /// </summary>
    /// <param name="unknown">
    ///  Receives the created object pointer when this method returns <see cref="HRESULT.S_OK"/>.
    /// </param>
    /// <returns>The HRESULT from <see cref="IClassFactory.CreateInstance(IUnknown*, Guid*, void**)"/>.</returns>
    /// <remarks>
    ///  <para>
    ///   On success, <paramref name="unknown"/> is caller-owned and represents one AddRef'd COM reference that the
    ///   caller must release.
    ///  </para>
    /// </remarks>
    internal HRESULT CreateInstance(out IUnknown* unknown)
    {
        unknown = default;
        fixed (IUnknown** u = &unknown)
        {
            return _classFactory->CreateInstance(null, IID.Get<IUnknown>(), (void**)u);
        }
    }

    /// <summary>
    ///  Creates a COM instance and returns a managed RCW for it.
    /// </summary>
    /// <param name="unknown">
    ///  Receives the managed object when creation succeeds; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>The HRESULT from <see cref="CreateInstance(out IUnknown*)"/>.</returns>
    /// <remarks>
    ///  <para>
    ///   This helper converts the native pointer by calling <see cref="RuntimeMarshal.GetObjectForIUnknown(nint)"/>.
    ///  </para>
    /// </remarks>
    internal HRESULT CreateInstance(out object? unknown)
    {
        HRESULT result = CreateInstance(out IUnknown* punk);
        unknown = punk is null ? null : RuntimeMarshal.GetObjectForIUnknown((nint)punk);
        return result;
    }

    /// <summary>
    ///  Releases the owned <see cref="IClassFactory"/> reference and unloads the module when this instance loaded it.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called by <see cref="Dispose()"/>.</param>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _classFactory->Release();
        if (_unloadModule && !_module.IsNull)
        {
            PInvoke.FreeLibrary(_module);
        }
    }
}