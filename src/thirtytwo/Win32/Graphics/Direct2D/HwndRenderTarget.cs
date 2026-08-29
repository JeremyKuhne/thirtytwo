// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  <see cref="HWND"/> render target.
/// </summary>
/// <devdoc>
///  <see href="https://learn.microsoft.com/windows/win32/Direct2D/supported-pixel-formats-and-alpha-modes#supported-formats-for-id2d1hwndrendertarget">
///   Supported Formats for ID2D1HwndRenderTarget
///  </see>
/// </devdoc>
public unsafe class HwndRenderTarget : RenderTarget, IPointer<ID2D1HwndRenderTarget>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1HwndRenderTarget"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1HwndRenderTarget* Pointer => (ID2D1HwndRenderTarget*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1HwndRenderTarget"/> pointer.
    /// </summary>
    /// <param name="renderTarget">The native render target pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public HwndRenderTarget(ID2D1HwndRenderTarget* renderTarget)
        : base((ID2D1RenderTarget*)renderTarget)
    {
    }

    /// <summary>
    ///  Creates a render target bound to a window handle.
    /// </summary>
    /// <typeparam name="TFactory">The factory wrapper type.</typeparam>
    /// <typeparam name="TWindow">The window wrapper type.</typeparam>
    /// <param name="factory">A Direct2D factory used to create the render target.</param>
    /// <param name="window">The target window.</param>
    /// <param name="size">The initial pixel size of the target surface.</param>
    /// <returns>A wrapper for the created <see cref="ID2D1HwndRenderTarget"/>.</returns>
    /// <remarks>
    ///  Uses default render target properties, which request the recommended BGRA format and ignore alpha for HWND targets.
    ///  A failed native call results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
    public static HwndRenderTarget CreateForWindow<TFactory, TWindow>(
        TFactory factory,
        TWindow window,
        Size size)
        where TFactory : IPointer<ID2D1Factory>
        where TWindow : IHandle<HWND> =>
        CreateForWindow(
            factory,
            window,
            new D2D_SIZE_U() { width = checked((uint)size.Width), height = checked((uint)size.Height) });

    /// <summary>
    ///  Creates a render target bound to a window handle.
    /// </summary>
    /// <typeparam name="TFactory">The factory wrapper type.</typeparam>
    /// <typeparam name="TWindow">The window wrapper type.</typeparam>
    /// <param name="factory">A Direct2D factory used to create the render target.</param>
    /// <param name="window">The target window.</param>
    /// <param name="size">The initial pixel size of the target surface.</param>
    /// <returns>A wrapper for the created <see cref="ID2D1HwndRenderTarget"/>.</returns>
    /// <remarks>
    ///  Uses default render target properties, which request the recommended BGRA format and ignore alpha for HWND targets.
    ///  A failed native call results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
    public static HwndRenderTarget CreateForWindow<TFactory, TWindow>(
        TFactory factory,
        TWindow window,
        D2D_SIZE_U size)
        where TFactory : IPointer<ID2D1Factory>
        where TWindow : IHandle<HWND>
    {
        // DXGI_FORMAT_B8G8R8A8_UNORM is the recommended pixel format for HwndRenderTarget for performance reasons.
        // DXGI_FORMAT_UNKNOWN and DXGI_FORMAT_UNKNOWN give DXGI_FORMAT_B8G8R8A8_UNORM and D2D1_ALPHA_MODE_IGNORE.
        D2D1_RENDER_TARGET_PROPERTIES properties = default;
        D2D1_HWND_RENDER_TARGET_PROPERTIES hwndProperties = new()
        {
            hwnd = window.Handle,
            pixelSize = size
        };

        ID2D1HwndRenderTarget* renderTarget;
        factory.Pointer->CreateHwndRenderTarget(
            &properties,
            &hwndProperties,
            &renderTarget).ThrowOnFailure();

        GC.KeepAlive(factory);
        GC.KeepAlive(window.Wrapper);

        return new HwndRenderTarget(renderTarget);
    }

    /// <summary>
    ///  Resizes the underlying HWND render target surface.
    /// </summary>
    /// <param name="size">The new pixel size.</param>
    /// <remarks>A failed native call results in exception flow from <c>ThrowOnFailure</c>.</remarks>
    public void Resize(Size size)
    {
        Pointer->Resize((D2D_SIZE_U)size).ThrowOnFailure();
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Converts an <see cref="HwndRenderTarget"/> wrapper to its underlying <see cref="ID2D1HwndRenderTarget"/> pointer.
    /// </summary>
    /// <param name="target">The wrapper instance.</param>
    /// <returns>The wrapped native render target pointer.</returns>
    public static implicit operator ID2D1HwndRenderTarget*(HwndRenderTarget target) => target.Pointer;
}