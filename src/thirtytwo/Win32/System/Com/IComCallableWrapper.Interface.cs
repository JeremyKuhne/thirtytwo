// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

public partial struct IComCallableWrapper
{
    /// <summary>
    ///  Used to flag that the COM object is a <see cref="ComWrappers"/> generated object.
    /// </summary>
    [ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("73B17DAF-0480-4702-AF7C-AF3BD4715D71")]
    public interface Interface
    {
    }
}