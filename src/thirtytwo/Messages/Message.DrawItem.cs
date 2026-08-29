// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>lParam</c> for <c>WM_DRAWITEM</c> as a native <c>DRAWITEMSTRUCT</c>.
    /// </summary>
    /// <param name="lParam">The message payload pointer to a <c>DRAWITEMSTRUCT</c> instance.</param>
    public readonly unsafe ref partial struct DrawItem(LPARAM lParam)
    {
        private readonly DRAWITEMSTRUCT* _drawItemStruct = (DRAWITEMSTRUCT*)lParam;

        /// <summary>
        ///  Gets the owner-draw source control type from <c>DRAWITEMSTRUCT.CtlType</c>.
        /// </summary>
        public ControlType Type => (ControlType)_drawItemStruct->CtlType;

        /// <summary>
        ///  Gets the source control identifier from <c>DRAWITEMSTRUCT.CtlID</c>.
        /// </summary>
        public uint ControlId => _drawItemStruct->CtlID;

        /// <summary>
        ///  Gets the item identifier from <c>DRAWITEMSTRUCT.itemID</c>.
        /// </summary>
        public uint ItemId => _drawItemStruct->itemID;

        /// <summary>
        ///  Gets action flags from <c>DRAWITEMSTRUCT.itemAction</c>.
        /// </summary>
        public Actions ItemAction => (Actions)_drawItemStruct->itemAction;

        /// <summary>
        ///  Gets state flags from <c>DRAWITEMSTRUCT.itemState</c>.
        /// </summary>
        public States ItemState => (States)_drawItemStruct->itemState;

        /// <summary>
        ///  Gets the item HWND from <c>DRAWITEMSTRUCT.hwndItem</c>.
        /// </summary>
        public HWND ItemWindow => _drawItemStruct->hwndItem;

        /// <summary>
        ///  Gets a device context wrapper for <c>DRAWITEMSTRUCT.hDC</c>.
        /// </summary>
        public DeviceContext DeviceContext => DeviceContext.Create(_drawItemStruct->hDC);

        /// <summary>
        ///  Gets the target item rectangle from <c>DRAWITEMSTRUCT.rcItem</c>.
        /// </summary>
        public Rectangle ItemRectangle => _drawItemStruct->rcItem;

        /// <summary>
        ///  Gets the application-defined item payload from <c>DRAWITEMSTRUCT.itemData</c>.
        /// </summary>
        public nuint ItemData => _drawItemStruct->itemData;
    }
}