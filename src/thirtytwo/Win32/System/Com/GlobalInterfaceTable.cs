// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Based on https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms.Primitives/src/Windows/Win32/Foundation/GlobalInterfaceTable.cs
//
// Original header
// ---------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Wrapper for the COM global interface table.
/// </summary>
internal static unsafe class GlobalInterfaceTable
{
    /// <summary>
    ///  Stores the process-wide global interface table pointer.
    /// </summary>
    private static readonly IGlobalInterfaceTable* s_globalInterfaceTable;

    static GlobalInterfaceTable()
    {
        Guid clsid = CLSID.StdGlobalInterfaceTable;
        fixed (IGlobalInterfaceTable** git = &s_globalInterfaceTable)
        {
            PInvoke.CoCreateInstance(
                &clsid,
                pUnkOuter: null,
                CLSCTX.CLSCTX_INPROC_SERVER,
                IID.Get<IGlobalInterfaceTable>(),
                (void**)git);
        }
    }

    /// <summary>
    ///  Registers the given <paramref name="interface"/> in the global interface table.
    /// </summary>
    /// <typeparam name="TInterface">The COM interface type to register.</typeparam>
    /// <param name="interface">Borrowed interface pointer to register.</param>
    /// <returns>The cookie used to refer to the interface in the table.</returns>
    /// <remarks>
    ///  <para>
    ///   The table takes its own reference internally. This method does not transfer ownership of
    ///   <paramref name="interface"/> and does not release the caller's reference.
    ///  </para>
    /// </remarks>
    public static uint RegisterInterface<TInterface>(TInterface* @interface)
        where TInterface : unmanaged, IComIID
    {
        uint cookie;
        s_globalInterfaceTable->RegisterInterfaceInGlobal(
            (IUnknown*)@interface,
            IID.Get<TInterface>(),
            &cookie);
        return cookie;
    }

    /// <summary>
    ///  <para>
    ///   Gets an agile interface for a previously registered cookie.
    ///  </para>
    /// </summary>
    /// <typeparam name="TInterface">
    ///  <para>
    ///   The COM interface type to resolve.
    ///  </para>
    /// </typeparam>
    /// <param name="cookie">
    ///  <para>
    ///   Registration cookie returned by <see cref="RegisterInterface{TInterface}(TInterface*)"/>.
    ///  </para>
    /// </param>
    /// <param name="result">
    ///  <para>
    ///   Receives the HRESULT from the lookup operation.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   A ComScope that owns one AddRef'd interface pointer when the lookup succeeds; otherwise a null scope.
    ///  </para>
    /// </returns>
    public static ComScope<TInterface> GetInterface<TInterface>(uint cookie, out HRESULT result)
        where TInterface : unmanaged, IComIID
    {
        ComScope<TInterface> @interface = new(null);
        result = s_globalInterfaceTable->GetInterfaceFromGlobal(cookie, IID.Get<TInterface>(), @interface);
        return @interface;
    }

    /// <summary>
    ///  Revokes the interface registered with <see cref="RegisterInterface{TInterface}(TInterface*)"/>.
    /// </summary>
    /// <param name="cookie">Registration cookie to revoke.</param>
    /// <returns>The HRESULT from the revoke operation.</returns>
    /// <remarks>
    ///  <para>
    ///   Revocation releases the table's internal reference for the registration.
    ///  </para>
    /// </remarks>
    public static HRESULT RevokeInterface(uint cookie)
    {
        HRESULT hr = s_globalInterfaceTable->RevokeInterfaceFromGlobal(cookie);
        Debug.Assert(hr.Succeeded);
        return hr;
    }
}