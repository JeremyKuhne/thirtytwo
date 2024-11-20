// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

/// <summary>
///  DeviceContext handle (HDC)
/// </summary>
/// <devdoc>
///  <see href="https://devblogs.microsoft.com/oldnewthing/20060601-06/?p=31003">What does the CS_OWNDC class style do?</see>
///  <see href="https://devblogs.microsoft.com/oldnewthing/20060602-00/?p=30993">What does the CS_CLASSDC class style do?</see>
///  <see href="https://learn.microsoft.com/windows/win32/gdi/display-device-context-defaults">Display Device Context Defaults</see>
///
///  Other things in consideration: Adding a BeginPaint that scales the HDC to the DPI of the window.
///
///    deviceContext.SetGraphicsMode(GRAPHICS_MODE.GM_ADVANCED);
///    uint dpi = hwnd.GetDpi();
///    Matrix3x2 transform = Matrix3x2.CreateScale((dpi / 96.0f) * 5.0f);
///    deviceContext.SetWorldTransform(ref transform);
/// </devdoc>
public unsafe readonly struct DeviceContext : IDisposable, IHandle<HDC>
{
    public HDC Handle { get; private init; }
    object? IHandle<HDC>.Wrapper => null;

    private readonly HWND HWND { get; init; }
    private readonly ContextState State { get; init; }

    /// <summary>
    ///  Creates a screen device context.
    /// </summary>
    public static DeviceContext Create() => new()
    {
        HWND = default,
        State = ContextState.UseRelease,
        Handle = Interop.GetDC(HWND.Null)
    };

    public static DeviceContext Create(
        HDC hdc,
        bool ownsHandle = false)
    {
        DeviceContext context = new()
        {
            HWND = default,
            State = ownsHandle ? ContextState.UseDelete : ContextState.DoNotRelease,
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
            State = ContextState.UseRelease,
            Handle = hdc.Handle
        };

        return context;
    }

    /// <inheritdoc cref="BeginPaint{THwnd}(THwnd, bool, out Rectangle)"/>
    public static DeviceContext BeginPaint<THwnd>(THwnd hwnd, bool saveContext = true)
        where THwnd : IHandle<HWND> => BeginPaint(hwnd, saveContext, out _);

    /// <summary>
    ///  Create a device context in a Begin/EndPaint scope.
    /// </summary>
    /// <param name="saveContext">
    ///  If <see langword="true"/>, the device context will be saved and restored.
    /// </param>
    public static DeviceContext BeginPaint<THwnd>(
        THwnd hwnd,
        bool saveContext,
        out Rectangle paintBounds)
        where THwnd : IHandle<HWND>
    {
        PAINTSTRUCT paintStruct = default;
        Interop.BeginPaint(hwnd.Handle, &paintStruct);
        paintBounds = paintStruct.rcPaint;
        if (saveContext)
        {
            int state = Interop.SaveDC(paintStruct.hdc);
            Debug.Assert(state != 0);
        }

        GC.KeepAlive(hwnd.Wrapper);
        return new()
        {
            HWND = hwnd.Handle,
            State = saveContext ? ContextState.UseEndPaint | ContextState.RestoreDc : ContextState.UseEndPaint,
            Handle = paintStruct.hdc
        };
    }

    public unsafe void Dispose()
    {
        if (State.HasFlag(ContextState.RestoreDc))
        {
            Interop.RestoreDC(Handle, -1);
        }

        Debug.Assert(State.AreAnyFlagsSet(ContextState.UseDelete | ContextState.UseRelease | ContextState.UseEndPaint | ContextState.DoNotRelease));

        if (State.HasFlag(ContextState.UseDelete))
        {
            if (!Interop.DeleteDC(new(Handle.Value)))
            {
                Debug.WriteLine("Failed to delete DC");
            }
        }
        else if (State.HasFlag(ContextState.UseRelease))
        {
            if (Interop.ReleaseDC(HWND, Handle) == 0)
            {
                Debug.WriteLine("Failed to release DC");
            }
        }
        else if (State.HasFlag(ContextState.UseEndPaint))
        {
            // This is all that matters for ending paint, we take advantage of this to not carry
            // the entire PAINTSTRUCT (it has a 32 byte array at the end of it).
            PAINTSTRUCT ps = new()
            {
                hdc = Handle
            };

            if (!Interop.EndPaint(HWND, &ps))
            {
                Debug.WriteLine("Failed to end paint");
            }
        }
    }

    public static implicit operator HDC(DeviceContext context) => context.Handle;
    public static unsafe explicit operator DeviceContext(WPARAM wparam) => Create(new((nint)wparam));

    [Flags]
    private enum ContextState
    {
        UseDelete           = 0b00000000_00000001,
        UseRelease          = 0b00000000_00000010,
        UseEndPaint         = 0b00000000_00000100,
        RestoreDc           = 0b00000000_00001000,
        DoNotRelease        = 0b00000000_00010000,
    }
}