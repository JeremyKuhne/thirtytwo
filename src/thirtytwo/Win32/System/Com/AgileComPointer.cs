// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Based on https://github.com/dotnet/winforms/ sources
//
// Original header
// ---------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Finalizable wrapper for COM pointers that gives agile access to the specified interface.
/// </summary>
public unsafe class AgileComPointer<TInterface> : IComPointer
    where TInterface : unmanaged, IComIID
{
    private readonly uint _cookie;
    private readonly TInterface* _originalHandle;

    public AgileComPointer(TInterface* @interface, bool takeOwnership)
    {
        _originalHandle = @interface;
        _cookie = GlobalInterfaceTable.RegisterInterface(@interface);

        // We let the GlobalInterfaceTable maintain the ref count here
        if (takeOwnership)
        {
            uint count = ((IUnknown*)@interface)->Release();
        }
    }

    /// <summary>
    ///  Gets the default interface.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NullReferenceException"/>
    public ComScope<TInterface> GetInterface()
    {
        var scope = GlobalInterfaceTable.GetInterface<TInterface>(_cookie, out HRESULT hr);
        hr.ThrowOnFailure();
        return scope;
    }

    public ComScope<TAsInterface> GetInterface<TAsInterface>()
        where TAsInterface : unmanaged, IComIID
    {
        var scope = TryGetInterface<TAsInterface>(out HRESULT hr);
        hr.ThrowOnFailure();
        return scope;
    }

    public ComScope<TInterface> TryGetInterface() => GlobalInterfaceTable.GetInterface<TInterface>(_cookie, out _);

    public ComScope<TInterface> TryGetInterface(out HRESULT hr)
        => GlobalInterfaceTable.GetInterface<TInterface>(_cookie, out hr);

    public ComScope<TAsInterface> TryGetInterface<TAsInterface>(out HRESULT hr)
        where TAsInterface : unmanaged, IComIID
    {
        var scope = GlobalInterfaceTable.GetInterface<TAsInterface>(_cookie, out hr);
        return scope;
    }

    ~AgileComPointer()
    {
        Debug.Fail($"Did not dispose {nameof(AgileComPointer<TInterface>)}");
        Dispose(disposing: false);
    }

    public bool Equals(TInterface* other) => other == _originalHandle;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        HRESULT hr = GlobalInterfaceTable.RevokeInterface(_cookie);
        if (disposing)
        {
            // Don't assert from the finalizer thread.
            Debug.Assert(hr.Succeeded);
        }
    }
}