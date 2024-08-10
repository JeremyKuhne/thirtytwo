// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Wrapper for the <see href="https://learn.microsoft.com/windows/win32/gdi/wm-ncpaint">frame painting message</see>.
    /// </summary>
    public readonly unsafe ref struct NonClientPaint(WPARAM wParam)
    {
        /// <summary>
        ///  Update region clipped to the window frame.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This may sometimes be <see cref="HRGN.Full"/> to indicate the entire window.
        ///  </para>
        ///  <para>
        ///   This is effectively <see cref="Interop.GetWindowRect(HWND, RECT*)"/>. If the window has a clipping region
        ///   set via <see cref="Interop.SetWindowRgn(HWND, HRGN, BOOL)"/>, that will be intersected with the rect.
        ///   This isn't very likely as having a clipping region
        ///   <see href="https://learn.microsoft.com/windows/win32/controls/cookbook-overview#when-visual-styles-are-not-applied">
        ///   prevents theming</see>.
        ///  </para>
        /// </remarks>
        public HRGN UpdateRegion => (HRGN)(nint)wParam;

        public HDC GetDC(HWND window, bool copyRegion = true)
        {
            // GetDCEx will take ownership of the region.
            HRGN updateRegion = UpdateRegion;
            if (updateRegion.IsFull)
            {
                updateRegion = HRGN.Null;
            }

            HRGN region = copyRegion && !updateRegion.IsNull ? updateRegion.Copy() : updateRegion;

            // GetDC (as opposed to GetDCEx) will call GetDCEx with a special flag that will set flags based
            // on the window's style. WS_CLIPCHILDREN will set DCX_CLIPCHILDREN for example.

            return Interop.GetDCEx(
                window,
                region,
                region.IsNull ? GET_DCX_FLAGS.DCX_WINDOW : GET_DCX_FLAGS.DCX_WINDOW | GET_DCX_FLAGS.DCX_INTERSECTRGN);
        }
    }
}