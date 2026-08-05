// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public readonly unsafe ref partial struct DrawItem(LPARAM lParam)
    {
        private readonly DRAWITEMSTRUCT* _drawItemStruct = (DRAWITEMSTRUCT*)lParam;

        public ControlType Type => (ControlType)_drawItemStruct->CtlType;
        public uint ControlId => _drawItemStruct->CtlID;
        public uint ItemId => _drawItemStruct->itemID;
        public Actions ItemAction => (Actions)_drawItemStruct->itemAction;
        public States ItemState => (States)_drawItemStruct->itemState;
        public HWND ItemWindow => _drawItemStruct->hwndItem;
        public DeviceContext DeviceContext => DeviceContext.Create(_drawItemStruct->hDC);
        public Rectangle ItemRectangle => _drawItemStruct->rcItem;
        public nuint ItemData => _drawItemStruct->itemData;
    }
}