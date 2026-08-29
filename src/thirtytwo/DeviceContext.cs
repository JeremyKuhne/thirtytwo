// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

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
///  deviceContext.SetGraphicsMode(GRAPHICS_MODE.GM_ADVANCED);
///  uint dpi = hwnd.GetDpi();
///  Matrix3x2 transform = Matrix3x2.CreateScale((dpi / 96.0f) * 5.0f);
///  deviceContext.SetWorldTransform(ref transform);
/// </devdoc>
public unsafe readonly partial struct DeviceContext : IDisposable, IHandle<HDC>
{
    /// <summary>
    ///  Gets the wrapped <c>HDC</c> handle.
    /// </summary>
    public HDC Handle { get; private init; }
    object? IHandle<HDC>.Wrapper => null;

    private readonly HWND HWND { get; init; }
    private readonly ContextState State { get; init; }

    /// <summary>
    ///  Creates a screen device context.
    /// </summary>
    /// <returns>A device context that must be disposed to release the screen DC.</returns>
    public static DeviceContext Create() => new()
    {
        HWND = default,
        State = ContextState.UseRelease,
        Handle = PInvoke.GetDC(HWND.Null)
    };

    /// <summary>
    ///  Creates a device context wrapper for an existing <c>HDC</c>.
    /// </summary>
    /// <param name="hdc">The device context handle to wrap.</param>
    /// <param name="ownsHandle">
    ///  <see langword="true"/> to delete the wrapped handle on dispose; otherwise disposal does not release it.
    /// </param>
    /// <returns>A wrapper over <paramref name="hdc"/>.</returns>
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

    /// <summary>
    ///  Creates a device context wrapper associated with a specific owner window.
    /// </summary>
    /// <typeparam name="THdc">The wrapped HDC handle-provider type.</typeparam>
    /// <typeparam name="THwnd">The wrapped HWND handle-provider type.</typeparam>
    /// <param name="hdc">The device context handle provider.</param>
    /// <param name="hwnd">The window that owns the DC for release semantics.</param>
    /// <returns>A wrapper that releases with <c>ReleaseDC</c> when disposed.</returns>
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
    /// <param name="hwnd">The window being painted.</param>
    /// <param name="saveContext">If <see langword="true"/>, the device context will be saved and restored.</param>
    /// <param name="paintBounds">The invalid rectangle reported by <c>BeginPaint</c>, in client pixels.</param>
    /// <returns>A paint-scoped device context that must be disposed to call <c>EndPaint</c>.</returns>
    public static DeviceContext BeginPaint<THwnd>(
        THwnd hwnd,
        bool saveContext,
        out Rectangle paintBounds)
        where THwnd : IHandle<HWND>
    {
        PAINTSTRUCT paintStruct = default;
        PInvoke.BeginPaint(hwnd.Handle, &paintStruct);
        paintBounds = paintStruct.rcPaint;
        if (saveContext)
        {
            int state = PInvoke.SaveDC(paintStruct.hdc);
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

    /// <summary>
    ///  Releases or restores the wrapped device context according to how it was created.
    /// </summary>
    public void Dispose()
    {
        if (State.HasFlag(ContextState.RestoreDc))
        {
            PInvoke.RestoreDC(Handle, -1);
        }

        Debug.Assert(State.AreAnyFlagsSet(ContextState.UseDelete | ContextState.UseRelease | ContextState.UseEndPaint | ContextState.DoNotRelease));

        if (State.HasFlag(ContextState.UseDelete))
        {
            if (!PInvoke.DeleteDC(new(Handle.Value)))
            {
                Debug.WriteLine("Failed to delete DC");
            }
        }
        else if (State.HasFlag(ContextState.UseRelease))
        {
            if (PInvoke.ReleaseDC(HWND, Handle) == 0)
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

            if (!PInvoke.EndPaint(HWND, &ps))
            {
                Debug.WriteLine("Failed to end paint");
            }
        }
    }

    /// <summary>
    ///  Converts this wrapper to its underlying <c>HDC</c> handle.
    /// </summary>
    /// <param name="context">The wrapper to convert.</param>
    /// <returns>The underlying <c>HDC</c> value.</returns>
    public static implicit operator HDC(DeviceContext context) => context.Handle;

    /// <summary>
    ///  Converts a <c>WPARAM</c> value containing an <c>HDC</c> to a wrapper instance.
    /// </summary>
    /// <param name="wparam">The message parameter containing an <c>HDC</c>.</param>
    /// <returns>A non-owning device-context wrapper over the extracted handle.</returns>
    public static explicit operator DeviceContext(WPARAM wparam) => Create(new((nint)wparam));
}