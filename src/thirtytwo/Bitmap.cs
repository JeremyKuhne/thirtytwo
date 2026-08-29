// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Wraps an <c>HBITMAP</c> handle with optional ownership semantics.
/// </summary>
public readonly struct Bitmap : IDisposable, IHandle<HBITMAP>
{
    /// <summary>
    ///  Gets the native bitmap handle.
    /// </summary>
    public HBITMAP Handle { get; private init; }
    object? IHandle<HBITMAP>.Wrapper => null;

    private readonly bool OwnsHandle { get; init; }

    /// <summary>
    ///  Creates a bitmap wrapper for an existing native handle.
    /// </summary>
    /// <param name="hbitmap">The bitmap handle to wrap.</param>
    /// <param name="ownsHandle">
    ///  <see langword="true"/> to delete <paramref name="hbitmap"/> when disposed; otherwise the caller retains ownership.
    /// </param>
    /// <returns>A wrapper over <paramref name="hbitmap"/>.</returns>
    public static Bitmap Create(HBITMAP hbitmap, bool ownsHandle = false) => new()
    {
        Handle = hbitmap,
        OwnsHandle = ownsHandle
    };

    /// <summary>
    ///  Converts a <see cref="Bitmap"/> wrapper to its <c>HBITMAP</c> handle.
    /// </summary>
    /// <param name="bitmap">The wrapper to convert.</param>
    /// <returns>The underlying <c>HBITMAP</c> value.</returns>
    public static implicit operator HBITMAP(in Bitmap bitmap) => bitmap.Handle;

    /// <summary>
    ///  Converts a <see cref="Bitmap"/> wrapper to <c>HGDIOBJ</c> for GDI APIs.
    /// </summary>
    /// <param name="bitmap">The wrapper to convert.</param>
    /// <returns>The underlying handle represented as <c>HGDIOBJ</c>.</returns>
    public static implicit operator HGDIOBJ(in Bitmap bitmap) => bitmap.Handle;

    /// <summary>
    ///  Releases the wrapped bitmap if this instance owns the handle.
    /// </summary>
    public void Dispose()
    {
        if (!Handle.IsNull && OwnsHandle)
        {
            PInvoke.DeleteObject(Handle);
        }
    }
}