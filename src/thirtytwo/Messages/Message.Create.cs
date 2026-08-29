// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

/// <summary>
///  Provides typed wrappers for interpreting Win32 message payloads.
/// </summary>
public static partial class Message
{
    /// <summary>
    ///  Interprets <c>lParam</c> for <c>WM_CREATE</c> as a native <c>CREATESTRUCTW</c>.
    /// </summary>
    /// <param name="lParam">The message payload pointer to a <c>CREATESTRUCTW</c> instance.</param>
    public readonly unsafe ref struct Create(LPARAM lParam)
    {
        private readonly CREATESTRUCTW* _createStruct = (CREATESTRUCTW*)(nint)lParam;

        /// <summary>
        ///  Gets the module instance handle from <c>CREATESTRUCTW.hInstance</c>.
        /// </summary>
        public HINSTANCE Instance => _createStruct->hInstance;

        /// <summary>
        ///  Gets the menu handle from <c>CREATESTRUCTW.hMenu</c>.
        /// </summary>
        public HMENU MenuHandle => _createStruct->hMenu;

        /// <summary>
        ///  Gets the parent window handle from <c>CREATESTRUCTW.hwndParent</c>.
        /// </summary>
        public HWND Parent => _createStruct->hwndParent;

        /// <summary>
        ///  Gets the initial window bounds from <c>CREATESTRUCTW</c> position and size fields.
        /// </summary>
        public Rectangle Bounds => new(_createStruct->x, _createStruct->y, _createStruct->cx, _createStruct->cy);

        /// <summary>
        ///  Gets the requested window title from <c>CREATESTRUCTW.lpszName</c>.
        /// </summary>
        public ReadOnlySpan<char> WindowName
            => Conversion.NullTerminatedStringToSpan((char*)_createStruct->lpszName);

        /// <summary>
        ///  Gets the class name when <c>CREATESTRUCTW.lpszClass</c> carries a string pointer.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Returns an empty span when the class is supplied as an atom.
        ///  </para>
        /// </remarks>
        public ReadOnlySpan<char> ClassName
            => (!_createStruct->lpszClass.IsNull && !ATOM.IsATOM((nint)_createStruct->lpszClass.Value))
                ? Conversion.NullTerminatedStringToSpan((char*)_createStruct->lpszClass)
            : default;

        /// <summary>
        ///  Gets the class atom when <c>CREATESTRUCTW.lpszClass</c> encodes an atom value.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Returns <see langword="default"/> when the class is supplied as a string.
        ///  </para>
        /// </remarks>
        public ATOM Atom
            => (!_createStruct->lpszClass.IsNull && ATOM.IsATOM((nint)_createStruct->lpszClass.Value))
                ? new ATOM((ushort)(nuint)_createStruct->lpszClass.Value) : default;
    }
}