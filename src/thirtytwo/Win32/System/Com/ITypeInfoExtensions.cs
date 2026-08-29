// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Extension helpers for <see cref="ITypeInfo"/>.
/// </summary>
public static unsafe class ITypeInfoExtensions
{
    /// <summary>
    ///  Provides extension helpers for a type information interface reference.
    /// </summary>
    /// <param name="typeInfo">The type information interface reference that extension members operate on.</param>
    extension(ref ITypeInfo typeInfo)
    {
        /// <summary>
        ///  Retrieves type attributes and wraps the result in a disposal scope.
        /// </summary>
        /// <param name="hr">Receives the result of <c>ITypeInfo::GetTypeAttr</c>.</param>
        /// <returns>
        ///  A scope that releases the type attributes through <c>ITypeInfo::ReleaseTypeAttr</c> when disposed.
        /// </returns>
        public ITypeInfoTypeAttrScope GetTypeAttr(out HRESULT hr)
        {
            hr = typeInfo.GetTypeAttr(out TYPEATTR* typeAttr);
            return new((ITypeInfo*)Unsafe.AsPointer(ref typeInfo), typeAttr);
        }
    }
}