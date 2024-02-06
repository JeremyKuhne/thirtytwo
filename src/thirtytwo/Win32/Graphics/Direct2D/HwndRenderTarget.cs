// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class HwndRenderTarget : RenderTarget, IPointer<ID2D1HwndRenderTarget>
{
    public new ID2D1HwndRenderTarget* Pointer { get; private set; }

    public HwndRenderTarget(ID2D1HwndRenderTarget* renderTarget)
        : base((ID2D1RenderTarget*)renderTarget)
    {
        Pointer = renderTarget;
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

    protected override void Dispose(bool disposing)
    {
        Pointer = null;
        base.Dispose(disposing);
    }
}