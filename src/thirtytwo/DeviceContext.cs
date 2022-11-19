// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

/// <summary>
///  DeviceContext handle (HDC)
/// </summary>
public readonly struct DeviceContext : IDisposable, IHandle<HDC>
{
    public HDC Handle { get; private init; }
    object? IHandle<HDC>.Wrapper => null;

    private readonly HWND HWND { get; init; }
    private readonly CollectionType Type { get; init; }

    public static DeviceContext Create(
        CreatedHDC hdc,
        bool ownsHandle = false)
    {
        DeviceContext context = new()
        {
            HWND = default,
            Type = ownsHandle ? CollectionType.Delete : CollectionType.None,
            Handle = hdc
        };

        return context;
    }

    public static DeviceContext Create(HDC hdc)
    {
        DeviceContext context = new()
        {
            HWND = default,
            Type = CollectionType.None,
            Handle = hdc
        };

        return context;
    }

    public static DeviceContext Create<THdc, THwnd>(
        THdc hdc,
        THwnd hwnd)
        where THwnd : IHandle<HWND>
        where THdc : IHandle<HDC>
    {
        DeviceContext context = new()
        {
            HWND = hwnd.Handle,
            Type = CollectionType.Release,
            Handle = hdc.Handle
        };

        return context;
    }

    public static DeviceContext Create<THwnd>(
        ref PAINTSTRUCT paintStruct,
        THwnd hwnd)
        where THwnd : IHandle<HWND>
    {
        DeviceContext context = new()
        {
            HWND = hwnd.Handle,
            Type = CollectionType.EndPaint,
            Handle = paintStruct.hdc
        };

        return context;
    }

    public unsafe void Dispose()
    {
        switch (Type)
        {
            case CollectionType.Delete:
                if (!Interop.DeleteDC(new(Handle.Value)))
                {
                    Debug.WriteLine("Failed to delete DC");
                }
                break;
            case CollectionType.Release:
                if (Interop.ReleaseDC(HWND, Handle) == 0)
                {
                    Debug.WriteLine("Failed to release DC");
                }
                break;
            case CollectionType.EndPaint:
                // This is all that matters for ending paint, we take advantage of this to not carry
                // the entire PAINTSTRUCT (it has a 32 byte array at the end of it.
                PAINTSTRUCT ps = new()
                {
                    hdc = Handle
                };

                if (!Interop.EndPaint(HWND, &ps))
                {
                    Debug.WriteLine("Failed to end paint");
                }
                break;
            case CollectionType.None:
                break;
            default:
                throw new InvalidOperationException();
        }
    }


    public static implicit operator HDC(DeviceContext context) => context.Handle;
    public static unsafe explicit operator DeviceContext(WPARAM wparam) => Create(new(new(wparam)));

    private enum CollectionType
    {
        None,
        Delete,
        Release,
        EndPaint
    }
}
