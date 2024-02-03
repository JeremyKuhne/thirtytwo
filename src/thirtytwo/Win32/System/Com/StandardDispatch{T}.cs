// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class for providing <see cref="IDispatch"/> services around an existing <see cref="ITypeInfo"/> for a
///  given <typeparamref name="T"/>.
/// </summary>
public unsafe abstract class StandardDispatch<T> : StandardDispatch
    where T : unmanaged, IComIID
{
    public StandardDispatch(ITypeInfo* typeInfo) : base(typeInfo, T.Guid)
    {
    }

    protected override ComScope GetComCallableWrapper() => new(ComHelpers.GetComPointer<T>(this));
}