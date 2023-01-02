// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.DotNet;

/// <summary>
///  This is intended to be a catch-all for replicating the interfaces provided on .NET CCWs. Right now this is mostly
///  just documenting the .NET CCW behavior.
/// </summary>
public unsafe abstract class DotNetDispatch :
    StandardDispatch,
    ISupportErrorInfo.Interface
{
    // .NET CCWs always support IMarshal, ISupportErrorInfo, IDispatchEx, IProvideClassInfo, and
    // IConnectionPointContainer. They also usually expose IAgileObject.
    //
    // On Exception objects the CCW also supports IErrorInfo.
    //
    // .NET Framework also supported the following interfaces, which are not implemented on .NET Core:
    //
    //      IManagedObject          - used .NET Remoting (not available on .NET Core)
    //      IObjectSafety           - for Code Access Security (not available on .NET Core)
    //      IWeakReferenceSource    - for WinRT
    //      ICustomPropertyProvider - for WinRT XAML (Jupiter)
    //      IReferenceTrackerTarget - for WinRT
    //      IStringable             - for WinRT

    // On .NET Core, IMarshal delegates to the standard IMarshal from CoCreateFreeThreadedMarshaler. It seems unlikely
    // that there would be client code that depends on being able to find this. Seems better and less risky to just
    // leave it off and let the default COM marshalling handle things.

    // IDispatchEx comes into play when *non-public* objects implement IReflect. All IDispatchEx info comes from and
    // goes to the IReflect interface. There is no cross-over between IDispatch and IDispatchEx info.

    // IProvideClassInfo is there, but only provides info if the class has a registered type library associated with
    // it. Otherwise it always returns TLBX_E_LIBNOTREGISTERED.

    // If [ComSourceInterfaces] IConnectionPointContainer will provide connection points for interfaces described in
    // the attribute. If the class isn't attributed it will still provide IConnectionPointContainer, but it will have
    // an empty IConnectionPoint enumerator.

    // IAgileObject is a flag interface that indicates that the object is free-threaded. With the interface applied the
    // GlobalInterfaceTable will skip marshalling and just do direct access in different appartments.

    // .NET always returns true for ISupportErrorInfo, this would only be relevant for our manual CCWs if we populated
    // IErrorInfo via Win32's SetErrorInfo().
    HRESULT ISupportErrorInfo.Interface.InterfaceSupportsErrorInfo(Guid* riid) => HRESULT.S_FALSE;
}