// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public static partial class Message
{
    public readonly unsafe ref struct Create(LPARAM lParam)
    {
        private readonly CREATESTRUCTW* _createStruct = (CREATESTRUCTW*)(nint)lParam;

        public HINSTANCE Instance => _createStruct->hInstance;
        public HMENU MenuHandle => _createStruct->hMenu;
        public HWND Parent => _createStruct->hwndParent;
        public Rectangle Bounds => new(_createStruct->x, _createStruct->y, _createStruct->cx, _createStruct->cy);

        public ReadOnlySpan<char> WindowName
            => Conversion.NullTerminatedStringToSpan((char*)_createStruct->lpszName);

        public ReadOnlySpan<char> ClassName
            => (!_createStruct->lpszClass.IsNull && !ATOM.IsATOM((nint)_createStruct->lpszClass.Value))
                ? Conversion.NullTerminatedStringToSpan((char*)_createStruct->lpszClass)
            : default;
        public ATOM Atom
            => (!_createStruct->lpszClass.IsNull && ATOM.IsATOM((nint)_createStruct->lpszClass.Value))
                ? new ATOM((ushort)(nuint)_createStruct->lpszClass.Value) : default;
    }
}