// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.DotNet;

public unsafe partial struct IManagedObject
{
    /// <summary>
    ///  Provides methods for controlling a managed object.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This was provided on CCWs on .NET Framework, but is not used in .NET Core as it used .NET Remoting to get
    ///   access to remote objects. (.NET Remoting is not available in Core.)
    ///  </para>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-ioi/0d0efe1d-a04d-433b-b9aa-efa6cf7dc148">
    ///    [MS-IOI]: IManagedObject Interface Protocol
    ///   </see>
    ///  </para>
    /// </remarks>
    [ComImport]
    [Guid("c3fcc19e-a970-11d2-8b5a-00a0c9b7c9c4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public unsafe interface Interface
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        [PreserveSig]
        HRESULT QueryInterface(Guid* riid, void** ppvObject);

        [PreserveSig]
        uint AddRef();

        [PreserveSig]
        uint Release();

        /// <summary>
        ///  Gets the string representation of this managed object.
        /// </summary>
        /// <param name="pBSTR">A pointer to a string that is the serialized object.</param>
        /// <remarks>
        ///  <para>
        ///   The <see cref="GetSerializedBuffer(BSTR*)"/> method serializes the object so it can be marshalled to
        ///   the client.
        ///  </para>
        /// </remarks>
        [PreserveSig]
        HRESULT GetSerializedBuffer(BSTR* pBSTR);

        /// <summary>
        ///  Gets the identity of this managed object.
        /// </summary>
        /// <param name="pBSTRGUID">A pointer to the GUID of the process in which the object resides.</param>
        /// <param name="AppDomainID">A pointer to the ID of the object's application domain.</param>
        /// <param name="pCCW">A pointer to object's index in the COM classic v-table.</param>
        [PreserveSig]
        HRESULT GetObjectIdentity(BSTR* pBSTRGUID, int* AppDomainID, int* pCCW);
#pragma warning restore SA1313
    }
}