// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;
public partial class WindowClass
{
    /// <summary>
    ///  Stores managed window-class registration data and marshals it to native WNDCLASSEXW.
    /// </summary>
    /// <param name="windowProcedure">The class window procedure delegate to register.</param>
    private unsafe partial class WindowClassInfo(WindowProcedure windowProcedure)
    {
        /// <summary>
        ///  Gets or sets the native structure size.
        /// </summary>
        public uint Size;

        /// <summary>
        ///  Gets or sets class style flags.
        /// </summary>
        public ClassStyle Style;

        /// <summary>
        ///  Gets or sets the class window procedure delegate.
        /// </summary>
        public WindowProcedure WindowProcedure = windowProcedure;

        /// <summary>
        ///  Gets or sets extra class bytes.
        /// </summary>
        public int ClassExtraBytes;

        /// <summary>
        ///  Gets or sets extra window bytes.
        /// </summary>
        public int WindowExtraBytes;

        /// <summary>
        ///  Gets or sets the module instance.
        /// </summary>
        public HINSTANCE Instance;

        /// <summary>
        ///  Gets or sets the large icon.
        /// </summary>
        public HICON Icon;

        /// <summary>
        ///  Gets or sets the default cursor.
        /// </summary>
        public HCURSOR Cursor;

        /// <summary>
        ///  Gets or sets the menu resource name.
        /// </summary>
        public string? MenuName;

        /// <summary>
        ///  Gets or sets the menu resource identifier.
        /// </summary>
        public int MenuId;

        /// <summary>
        ///  Gets or sets the class name.
        /// </summary>
        public string? ClassName;

        /// <summary>
        ///  Gets or sets the class atom.
        /// </summary>
        public ATOM ClassAtom;

        /// <summary>
        ///  Gets or sets the small icon.
        /// </summary>
        public HICON SmallIcon;

        /// <summary>
        ///  Gets or sets the class background brush.
        /// </summary>
        public HBRUSH Background;

        /// <summary>
        ///  Converts a native <c>WNDCLASSEXW</c> structure to managed class metadata.
        /// </summary>
        /// <param name="nativeClass">The native class descriptor.</param>
        /// <returns>The managed representation.</returns>
        public static implicit operator WindowClassInfo(WNDCLASSEXW nativeClass)
        {
            var windowClass = new WindowClassInfo(
                Marshal.GetDelegateForFunctionPointer<WindowProcedure>((nint)nativeClass.lpfnWndProc))
            {
                Size = nativeClass.cbSize,
                Style = (ClassStyle)nativeClass.style,
                ClassExtraBytes = nativeClass.cbClsExtra,
                WindowExtraBytes = nativeClass.cbWndExtra,
                Instance = nativeClass.hInstance,
                Icon = nativeClass.hIcon,
                Cursor = nativeClass.hCursor,
                Background = nativeClass.hbrBackground,
                SmallIcon = nativeClass.hIconSm,
            };

            if (!nativeClass.lpszMenuName.IsNull)
            {
                if (INTRESOURCE.IsIntResource((char*)nativeClass.lpszMenuName))
                {
                    windowClass.MenuId = (ushort)(char*)nativeClass.lpszMenuName;
                }
                else
                {
                    windowClass.MenuName = new string((char*)nativeClass.lpszMenuName);
                }
            }

            if (!nativeClass.lpszClassName.IsNull)
            {
                nint value = (nint)(char*)nativeClass.lpszClassName;
                if (ATOM.IsATOM(value))
                {
                    windowClass.ClassAtom = (ushort)value;
                }
                else
                {
                    windowClass.ClassName = new string((char*)nativeClass.lpszClassName);
                }
            }

            return windowClass;
        }

        /// <summary>
        ///  Registers the described class with USER32.
        /// </summary>
        /// <returns>The registered class atom.</returns>
        public ATOM Register()
        {
            fixed (char* cn = ClassName)
            fixed (char* mn = MenuName)
            {
                WNDCLASSEXW wndclass = new()
                {
                    cbSize = (uint)sizeof(WNDCLASSEXW),
                    style = (WNDCLASS_STYLES)Style,
                    lpfnWndProc = (WNDPROC)Marshal.GetFunctionPointerForDelegate(WindowProcedure),
                    cbClsExtra = ClassExtraBytes,
                    cbWndExtra = WindowExtraBytes,
                    hInstance = Instance,
                    hIcon = Icon,
                    hCursor = Cursor,
                    hbrBackground = Background,
                    hIconSm = SmallIcon,
                    lpszClassName = cn is null ? (PCWSTR)(char*)ClassAtom.Value : cn,
                    lpszMenuName = mn is null ? (PCWSTR)(char*)MenuId : mn
                };

                ATOM result = PInvoke.RegisterClassEx(&wndclass);
                if (!result.IsValid)
                {
                    Error.GetLastError().ThrowThirtyTwoException();
                }

                return result;
            }
        }
    }
}