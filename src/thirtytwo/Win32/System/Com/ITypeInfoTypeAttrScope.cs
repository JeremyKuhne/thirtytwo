// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Scope wrapper for a <see cref="TYPEATTR"/> pointer returned from <c>ITypeInfo::GetTypeAttr</c>.
/// </summary>
/// <remarks>
///  <para>
///   Disposing this scope releases the attribute pointer through the associated <see cref="ITypeInfo"/> instance.
///  </para>
/// </remarks>
public readonly unsafe ref struct ITypeInfoTypeAttrScope
{
    private readonly ITypeInfo* _typeInfo;
    private readonly TYPEATTR* _typeAttr;

    /// <summary>
    ///  Initializes a scope for a type attribute pointer.
    /// </summary>
    /// <param name="typeInfo">The <see cref="ITypeInfo"/> that owns <paramref name="typeAttr"/>.</param>
    /// <param name="typeAttr">The type attribute pointer to release on disposal.</param>
    public ITypeInfoTypeAttrScope(ITypeInfo* typeInfo, TYPEATTR* typeAttr)
    {
        _typeInfo = typeInfo;
        _typeAttr = typeAttr;
    }

    /// <summary>
    ///  Gets the underlying <see cref="TYPEATTR"/> pointer.
    /// </summary>
    public TYPEATTR* Value => _typeAttr;

    /// <summary>
    ///  Releases the wrapped type attributes when present.
    /// </summary>
    public void Dispose()
    {
        if (_typeAttr is not null)
        {
            _typeInfo->ReleaseTypeAttr(_typeAttr);
        }
    }
}