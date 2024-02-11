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
    public new ID2D1HwndRenderTarget* Pointer => (ID2D1HwndRenderTarget*)base.Pointer;

    public HwndRenderTarget(ID2D1HwndRenderTarget* renderTarget)
        : base((ID2D1RenderTarget*)renderTarget)
    {
    }

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

    public static implicit operator ID2D1HwndRenderTarget*(HwndRenderTarget target) => target.Pointer;
}