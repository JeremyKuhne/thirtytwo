// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class for providing <see cref="IDispatch"/> services around an existing <see cref="ITypeInfo"/> for a
///  given <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The primary COM interface type used for CCW callbacks.</typeparam>
public unsafe abstract class StandardDispatch<T> : StandardDispatch
    where T : unmanaged, IComIID
{
    /// <summary>
    ///  Initializes a dispatch wrapper with the specified type information.
    /// </summary>
    /// <param name="typeInfo">The backing <see cref="ITypeInfo"/> pointer.</param>
    public StandardDispatch(ITypeInfo* typeInfo) : base(typeInfo, T.Guid)
    {
    }

    /// <inheritdoc/>
    protected override ComScope GetComCallableWrapper() => new(this.GetComPointer<T>());
}