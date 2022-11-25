// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;
public partial class WindowClass
{
    private unsafe partial class WindowClassInfo
    {
        public uint Size;
        public ClassStyle Style;
        public WindowProcedure WindowProcedure;
        public int ClassExtraBytes;
        public int WindowExtraBytes;
        public HINSTANCE Instance;
        public HICON Icon;
        public HCURSOR Cursor;
        public string? MenuName;
        public int MenuId;
        public string? ClassName;
        public ATOM ClassAtom;
        public HICON SmallIcon;
        public HBRUSH Background;

        public WindowClassInfo(WindowProcedure windowProcedure)
        {
            WindowProcedure = windowProcedure;
        }

        public static implicit operator WindowClassInfo(WNDCLASSEXW nativeClass)
        {
            var windowClass = new WindowClassInfo(
                Marshal.GetDelegateForFunctionPointer<WindowProcedure>((IntPtr)nativeClass.lpfnWndProc))
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

                ATOM result = Interop.RegisterClassEx(&wndclass);
                if (!result.IsValid)
                {
                    Error.GetLastError().Throw();
                }

                return result;
            }
        }
    }
}