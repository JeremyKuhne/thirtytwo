// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics;

public unsafe abstract class DirectDrawBase<T> : DisposableBase.Finalizable, IPointer<T> where T : unmanaged
{
    private nint _pointer;

    public T* Pointer => (T*)_pointer;

    public DirectDrawBase(T* pointer)
    {
        if (pointer is null)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        _pointer = (nint)pointer;
    }

    public static implicit operator T*(DirectDrawBase<T> d) => d.Pointer;

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