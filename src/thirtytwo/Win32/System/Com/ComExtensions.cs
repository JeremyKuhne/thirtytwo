// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using static System.Runtime.InteropServices.ComWrappers;

namespace Windows.Win32;

public static unsafe class ComExtensions
{
    extension(object? obj)
    {
        /// <summary>
        ///  Gets the specified <typeparamref name="T"/> interface for the object.
        /// </summary>
        internal T* GetComPointer<T>() where T : unmanaged, IComIID
        {
            T* result = obj.TryGetComPointer<T>(out HRESULT hr);
            hr.ThrowOnFailure();
            return result;
        }

        /// <summary>
        ///  Attempts to get the specified <typeparamref name="T"/> interface for the object.
        /// </summary>
        internal T* TryGetComPointer<T>(out HRESULT result) where T : unmanaged, IComIID
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
                result = PInvoke.E_NOINTERFACE;
                return null;
            }

            if (typeof(T) == typeof(IUnknown))
            {
                // No need to query if we wanted IUnknown.
                result = HRESULT.S_OK;
                return (T*)ccw;
            }

            // Now query out the requested interface
            void* ppvObject;
            result = ccw->QueryInterface(IID.Get<T>(), &ppvObject);
            ccw->Release();
            return (T*)ppvObject;
        }
    }

    /// <summary>
    ///  For the given <paramref name="this"/> pointer unwrap the associated managed object and use it to
    ///  invoke <paramref name="func"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Handles exceptions and converts to <c>HRESULT</c>.
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

    extension(Guid classId)
    {
        /// <summary>
        ///  Creates the COM class identified by the GUID.
        /// </summary>
        /// <exception cref="COMException">Thrown if the class can't be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the class can't be created.</exception>
        /// <returns><see cref="IUnknown"/> for the class. Throws if unable to create the class.</returns>
        public IUnknown* CreateComClass()
        {
            Guid* rclsid = &classId;
            IUnknown* unknown = CreateWithIClassFactory2();

            if (unknown is null)
            {
                HRESULT hr = PInvoke.CoCreateInstance(
                    rclsid,
                    null,
                    CLSCTX.CLSCTX_INPROC_SERVER,
                    IID.Get<IUnknown>(),
                    (void**)&unknown);
                hr.ThrowOnFailure();
            }

            return unknown;

            IUnknown* CreateWithIClassFactory2()
            {
                using ComScope<IClassFactory2> factory = new(null);

                HRESULT hr = PInvoke.CoGetClassObject(
                    rclsid,
                    CLSCTX.CLSCTX_INPROC_SERVER,
                    null,
                    IID.Get<IClassFactory2>(),
                    factory);

                if (hr.Failed)
                {
                    Debug.Assert(hr == PInvoke.E_NOINTERFACE);
                    return null;
                }

                LICINFO info = new()
                {
                    cbLicInfo = sizeof(LICINFO)
                };

                factory.Pointer->GetLicInfo(&info);
                if (info.fRuntimeKeyAvail)
                {
                    using BSTR key = default;
                    factory.Pointer->RequestLicKey(0, &key);
                    Guid iid = IUnknown.IID_Guid;
                    factory.Pointer->CreateInstanceLic(null, in iid, key, out void* unknown);
                    return (IUnknown*)unknown;
                }
                else
                {
                    void* unknown;
                    factory.Pointer->CreateInstance(null, IID.Get<IUnknown>(), &unknown);
                    return (IUnknown*)unknown;
                }
            }
        }

        /// <summary>
        ///  Finds an interface's <see cref="ITypeInfo"/> in the type library identified by the GUID.
        /// </summary>
        public ComScope<ITypeInfo> GetRegisteredTypeInfo(
            ushort majorVersion,
            ushort minorVersion,
            Guid interfaceId)
        {
            // Load the registered type library and get the relevant ITypeInfo for the specified interface.
            using ComScope<ITypeLib> typelib = new(null);
            PInvoke.LoadRegTypeLib(classId, majorVersion, minorVersion, 0, typelib).ThrowOnFailure();

            ComScope<ITypeInfo> typeInfo = new(null);
            typelib.Pointer->GetTypeInfoOfGuid(interfaceId, typeInfo).ThrowOnFailure();
            return typeInfo;
        }
    }
}