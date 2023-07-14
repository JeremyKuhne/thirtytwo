// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public readonly struct Bitmap : IDisposable, IHandle<HBITMAP>
{
    public HBITMAP Handle { get; private init; }
    object? IHandle<HBITMAP>.Wrapper => null;

    private readonly bool OwnsHandle { get; init; }

    public static Bitmap Create(HBITMAP hbitmap, bool ownsHandle = false) => new()
    {
        Handle = hbitmap,
        OwnsHandle = ownsHandle
    };

    public static implicit operator HBITMAP(in Bitmap bitmap) => bitmap.Handle;
    public static implicit operator HGDIOBJ(in Bitmap bitmap) => bitmap.Handle;

    public void Dispose()
    {
        if (!Handle.IsNull && OwnsHandle)
        {
            Interop.DeleteObject(Handle);
        }
    }
}