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
public sealed unsafe class AgileComPointer<TInterface> : IDisposable
    where TInterface : unmanaged, IComIID
{
    private readonly uint _cookie;

    public AgileComPointer(TInterface* @interface)
    {
        _cookie = GlobalInterfaceTable.RegisterInterface(@interface);

        // We let the GlobalInterfaceTable maintain the ref count here
        uint count = ((IUnknown*)@interface)->Release();
        Debug.Assert(count == 1);
    }

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

    public ComScope<TAsInterface> TryGetInterface<TAsInterface>(out HRESULT hr)
        where TAsInterface : unmanaged, IComIID
    {
        var scope = GlobalInterfaceTable.GetInterface<TAsInterface>(_cookie, out hr);
        return scope;
    }

    ~AgileComPointer()
    {
        Debug.Fail($"Did not dispose {nameof(AgileComPointer<TInterface>)}");
        Dispose();
    }

    public void Dispose()
    {
        HRESULT hr = GlobalInterfaceTable.RevokeInterface(_cookie);
        Debug.Assert(hr.Succeeded);
        GC.SuppressFinalize(this);
    }
}