// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal sealed unsafe class RawWindowClass : IDisposable
{
    private readonly HMODULE _module;
    private readonly string _className;
    private readonly HBRUSH _backgroundBrush;
    private readonly WindowProcedure _windowProcedure;
    private bool _disposed;

    internal RawWindowClass(HMODULE module, string className, Color backgroundColor)
    {
        _module = module;
        _className = className;
        _windowProcedure = WindowProcedure;
        _backgroundBrush = PInvoke.CreateSolidBrush((COLORREF)backgroundColor);
        if (_backgroundBrush.IsNull)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        try
        {
            fixed (char* classNamePointer = className)
            {
                WNDCLASSEXW windowClass = new()
                {
                    cbSize = (uint)sizeof(WNDCLASSEXW),
                    lpfnWndProc = (WNDPROC)Marshal.GetFunctionPointerForDelegate(_windowProcedure),
                    hInstance = module,
                    hbrBackground = _backgroundBrush,
                    lpszClassName = classNamePointer
                };

                ATOM atom = PInvoke.RegisterClassEx(&windowClass);
                if (!atom.IsValid)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }
            }
        }
        catch
        {
            _ = PInvoke.DeleteObject(_backgroundBrush);
            throw;
        }
    }

    internal HWND CreateChild(HWND parent, Rectangle bounds, WINDOW_STYLE style)
    {
        HWND window = PInvoke.CreateWindowEx(
            default,
            _className,
            string.Empty,
            style,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            parent,
            HMENU.Null,
            _module,
            null);
        if (window.IsNull)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return window;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (PInvoke.UnregisterClass(_className, _module))
        {
            _ = PInvoke.DeleteObject(_backgroundBrush);
        }
    }

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
        => PInvoke.DefWindowProc(window, message, wParam, lParam);
}