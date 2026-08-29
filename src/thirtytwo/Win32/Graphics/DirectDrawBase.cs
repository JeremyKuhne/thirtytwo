// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics;

/// <summary>
///  Base class for DirectDraw objects.
/// </summary>
/// <remarks>
///  <para>
///   Instances own a COM-style interface pointer and release it exactly once during disposal/finalization.
///  </para>
/// </remarks>
/// <devdoc>
///  <see href="https://learn.microsoft.com/archive/msdn-magazine/2009/june/introducing-direct2d"/>
///
///  Direct3D, DirectWrite, and Direct2D use a lightweight version of the COM specification to manage object lifetime
///  through interfaces derived from IUnknown. There's no need to initialize the COM run time and worry about apartments
///  or proxies. It's just a convention to simplify resource management and allow APIs and applications to expose and
///  consume objects in a well-defined way.
/// </devdoc>
public unsafe abstract class DirectDrawBase<T> : DisposableBase.Finalizable, IPointer<T> where T : unmanaged
{
    private nint _pointer;

    /// <summary>
    ///  Gets the underlying unmanaged interface pointer.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Returns <see langword="null"/> after disposal.
    ///  </para>
    /// </remarks>
    public T* Pointer => (T*)_pointer;

    /// <summary>
    ///  Initializes a new wrapper that takes ownership of <paramref name="pointer"/>.
    /// </summary>
    /// <param name="pointer">Non-null interface pointer to manage.</param>
    public DirectDrawBase(T* pointer)
    {
        if (pointer is null)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        _pointer = (nint)pointer;
    }

    /// <summary>
    ///  Gets the wrapped unmanaged pointer.
    /// </summary>
    /// <param name="d">Wrapper instance.</param>
    /// <returns>The current unmanaged pointer value.</returns>
    public static implicit operator T*(DirectDrawBase<T> d) => d.Pointer;

    /// <summary>
    ///  Releases the owned interface pointer.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="true"/> when called from <c>Dispose()</c>; <see langword="false"/> during finalization.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        // DirectDraw objects can be accessed from any thread.
        nint current = Interlocked.Exchange(ref _pointer, 0);
        if (current != 0)
        {
            ((IUnknown*)current)->Release();
        }
    }
}