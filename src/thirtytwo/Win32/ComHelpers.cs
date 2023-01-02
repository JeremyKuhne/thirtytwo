// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using static System.Runtime.InteropServices.ComWrappers;

namespace Windows.Win32;

internal static unsafe partial class ComHelpers
{
    /// <summary>
    ///  Gets the specified <typeparamref name="T"/> interface for the given <paramref name="obj"/>.
    /// </summary>
    internal static T* GetComPointer<T>(object? obj) where T : unmanaged, IComIID
    {
        T* result = TryGetComPointer<T>(obj, out HRESULT hr);
        hr.ThrowOnFailure();
        return result;
    }

    /// <summary>
    ///  Attempts to get the specified <typeparamref name="T"/> interface for the given <paramref name="obj"/>.
    /// </summary>
    internal static T* TryGetComPointer<T>(object? obj, out HRESULT result) where T : unmanaged, IComIID
    {
        if (obj is null)
        {
            result = HRESULT.E_POINTER;
            return null;
        }

        IUnknown* ccw = CustomComWrappers.GetComInterfaceForObject(obj);
        if (ccw is null)
        {
            // Not handled, fall back to classic COM interop methods.
            try
            {
                ccw = (IUnknown*)Marshal.GetIUnknownForObject(obj);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Did not find IUnknown for {obj.GetType().Name}. {ex.Message}");
            }
        }

        if (ccw is null)
        {
            result = HRESULT.E_NOINTERFACE;
            return null;
        }

        if (typeof(T) == typeof(IUnknown))
        {
            // No need to query if we wanted IUnknown.
            result = HRESULT.S_OK;
            return (T*)ccw;
        }

        // Now query out the requested interface
        result = ccw->QueryInterface(IID.GetRef<T>(), out void* ppvObject);
        ccw->Release();
        return (T*)ppvObject;
    }

    /// <summary>
    ///  For the given <paramref name="this"/> pointer unwrap the associated managed object and use it to
    ///  invoke <paramref name="func"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Handles exceptions and converts to <see cref="HRESULT"/>.
    ///  </para>
    /// </remarks>
    internal static HRESULT UnwrapAndInvoke<TThis, TInterface>(TThis* @this, Func<TInterface, HRESULT> func)
        where TThis : unmanaged, IComIID
        where TInterface : class
    {
        try
        {
            TInterface? @object = ComInterfaceDispatch.GetInstance<TInterface>((ComInterfaceDispatch*)@this);
            return @object is null ? HRESULT.COR_E_OBJECTDISPOSED : func(@object);
        }
        catch (Exception ex)
        {
            return (HRESULT)ex.HResult;
        }
    }

    /// <summary>
    ///  For the given <paramref name="this"/> pointer unwrap the associated managed object and use it to
    ///  invoke <paramref name="func"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Handles exceptions and converts to <see langword="default"/>.
    ///  </para>
    /// </remarks>
    internal static TReturn? UnwrapAndInvoke<TThis, TInterface, TReturn>(TThis* @this, Func<TInterface, TReturn> func)
        where TThis : unmanaged, IComIID
        where TInterface : class
    {
        try
        {
            TInterface? @object = ComInterfaceDispatch.GetInstance<TInterface>((ComInterfaceDispatch*)@this);
            return @object is null ? default : func(@object);
        }
        catch (Exception ex)
        {
            Debug.Fail(ex.Message);
            return default;
        }
    }

    /// <summary>
    ///  Creates the given <paramref name="classId"/>.
    /// </summary>
    /// <param name="classId">The class guid.</param>
    /// <exception cref="COMException">Thrown if the class can't be created.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the class can't be created.</exception>
    /// <returns><see cref="IUnknown"/> for the class. Throws if unable to create the class.</returns>
    public static IUnknown* CreateComClass(Guid classId)
    {
        Guid* rclsid = &classId;
        IUnknown* unknown = CreateWithIClassFactory2();

        if (unknown is null)
        {
            HRESULT hr = Interop.CoCreateInstance(rclsid, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.Get<IUnknown>(), (void**)&unknown);
            hr.ThrowOnFailure();
        }

        return unknown;

        IUnknown* CreateWithIClassFactory2()
        {
            using ComScope<IClassFactory2> factory = new(null);

            HRESULT hr = Interop.CoGetClassObject(rclsid, CLSCTX.CLSCTX_INPROC_SERVER, null, IID.Get<IClassFactory2>(), factory);

            if (hr.Failed)
            {
                Debug.Assert(hr == HRESULT.E_NOINTERFACE);
                return null;
            }

            LICINFO info = new()
            {
                cbLicInfo = sizeof(LICINFO)
            };

            factory.Value->GetLicInfo(&info);
            if (info.fRuntimeKeyAvail)
            {
                using BSTR key = default;
                factory.Value->RequestLicKey(0, &key);
                factory.Value->CreateInstanceLic(null, null, IID.GetRef<IUnknown>(), key, out void* unknown);
                return (IUnknown*)unknown;
            }
            else
            {
                factory.Value->CreateInstance(null, IID.GetRef<IUnknown>(), out void* unknown);
                return (IUnknown*)unknown;
            }
        }
    }

    public static void PopulateIUnknown<TComInterface>(IUnknown.Vtbl* vtable)
        where TComInterface : unmanaged, IComIID
        => PopulateIUnknownImpl<TComInterface>(vtable);

    static partial void PopulateIUnknownImpl<TComInterface>(IUnknown.Vtbl* vtable) where TComInterface : unmanaged, IComIID;
}

// This is temporary to illustrate the feature request in https://github.com/microsoft/CsWin32/issues/831
internal static unsafe partial class ComHelpers
{
    static partial void PopulateIUnknownImpl<TComInterface>(IUnknown.Vtbl* vtable) where TComInterface : unmanaged, IComIID
    {
        // Custom behavior for specific COM CCWs can be done by checking typeof(TComInterface) against a specific
        // interface, such as typeof(IAccessible).
        CustomComWrappers.PopulateIUnknown(vtable);
    }
}