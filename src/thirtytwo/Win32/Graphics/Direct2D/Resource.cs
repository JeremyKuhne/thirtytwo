// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1Resource"/>.
/// </summary>
public unsafe class Resource : DirectDrawBase<ID2D1Resource>, IPointer<ID2D1Resource>
{
    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1Resource"/> pointer.
    /// </summary>
    /// <param name="resource">The native Direct2D resource pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public Resource(ID2D1Resource* resource) : base(resource)
    {
    }
}