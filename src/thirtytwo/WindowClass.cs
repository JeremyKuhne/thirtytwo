// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;

public unsafe partial class WindowClass : DisposableBase.Finalizable
{
    private readonly WNDPROC _priorClassProcedure;
    // Stash the delegate to keep it from being collected
    private readonly WindowProcedure _windowProcedure;
    private readonly string _className;
    private readonly WindowClassInfo? _windowClass;
    private readonly object _lock = new();

    [ThreadStatic]
    private static WindowProcedure? t_initializeProcedure;

    /// <summary>
    ///  The atom for the class if <see cref="WindowClass"/> did the class registration.
    /// </summary>
    public ATOM Atom { get; private set; }

    /// <summary>
    ///  The module instance that this class is registered with. This will be null when this class wraps an
    ///  existing registered class (such as common control classes).
    /// </summary>
    public HMODULE ModuleInstance { get; }

    /// <summary>
    ///  Construct a new <see cref="WindowClass"/>.
    /// </summary>
    /// <param name="className">
    ///  The name of the class. If not provided a new GUID will be used.
    /// </param>
    /// <param name="moduleInstance">
    ///  The module instance to register the class with. If not provided the module instance
    ///  of the launching executable will be used.
    /// </param>
    /// <param name="backgroundBrush">
    ///  The background brush for the window. If not provided the system window color will be used. To have no
    ///  background brush, pass <see cref="HBRUSH.Invalid"/>.
    /// </param>
    /// <param name="icon">
    ///  The icon for the window. If not provided the application icon will be used.
    /// </param>
    /// <param name="smallIcon">
    ///  The small icon for the window. If not provided the application icon will be used.
    /// </param>
    /// <param name="cursor">
    ///  The default cursor to use. If not provided the arrow cursor will be used.
    /// </param>
    /// <param name="menuName">
    ///  The name of the menu to be used, if any. Cannot also specify <paramref name="menuId"/>
    /// </param>
    /// <param name="menuId">
    ///  The id of the menu to be used, if any. Cannot also specify <paramref name="menuName"/>
    /// </param>
    /// <param name="classExtraBytes">
    ///  The number of extra bytes to be allocated for the class, if any.
    /// </param>
    /// <param name="windowExtraBytes">
    ///  The number of extra bytes to be allocated for the window, if any.
    /// </param>
    /// <exception cref="ArgumentException">
    ///  <paramref name="menuName"/> and <paramref name="menuId"/> were both specified.
    /// </exception>
    public unsafe WindowClass(
        string? className = default,
        HMODULE moduleInstance = default,
        ClassStyle classStyle = ClassStyle.HorizontalRedraw | ClassStyle.VerticalRedraw,
        HBRUSH backgroundBrush = default,
        HICON icon = default,
        HICON smallIcon = default,
        HCURSOR cursor = default,
        string? menuName = null,
        int menuId = 0,
        int classExtraBytes = 0,
        int windowExtraBytes = 0)
    {
        // Handle default values
        className ??= Guid.NewGuid().ToString();

        // Why do I have to add 1 to the color index when I set it as the hbrBackground of a window class?
        // https://devblogs.microsoft.com/oldnewthing/20140305-00/?p=1593
        if (backgroundBrush.IsNull)
        {
            backgroundBrush = SystemColor.Window;
        }
        else if (backgroundBrush == HBRUSH.Invalid)
        {
            backgroundBrush = default;
        }

        if (icon == default)
        {
            icon = IconId.Application;
        }
        else if (icon == HICON.Invalid)
        {
            icon = default;
        }

        if (cursor == default)
        {
            cursor = CursorId.Arrow;
        }
        else if (cursor == HCURSOR.Invalid)
        {
            cursor = default;
        }

        if (moduleInstance.IsNull)
        {
            moduleInstance = HMODULE.GetLaunchingExecutable();
        }

        if (menuId != 0 && menuName is not null)
        {
            throw new ArgumentException($"Can't set both {nameof(menuName)} and {nameof(menuId)}.");
        }

        _windowProcedure = WindowProcedureInternal;
        ModuleInstance = moduleInstance;

        _className = className;
        _windowClass = new WindowClassInfo(_windowProcedure)
        {
            ClassName = className,
            Style = classStyle,
            ClassExtraBytes = classExtraBytes,
            WindowExtraBytes = windowExtraBytes,
            Instance = moduleInstance,
            Icon = icon,
            Cursor = cursor,
            Background = backgroundBrush,
            SmallIcon = smallIcon,
            MenuId = menuId,
            MenuName = menuName
        };
    }

    public WindowClass(string registeredClassName)
    {
        _windowProcedure = WindowProcedureInternal;
        _className = registeredClassName;
        ModuleInstance = HMODULE.Null;

        // We need to subclass the preexisting window class to get class messages first and during construction.
        HWND window = CreateWindow();
        try
        {
            _priorClassProcedure = (WNDPROC)window.GetClassLong(GET_CLASS_LONG_INDEX.GCL_WNDPROC);
            window.SetClassLong(GET_CLASS_LONG_INDEX.GCL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_windowProcedure));
        }
        finally
        {
            Interop.DestroyWindow(window);
        }
    }

    public bool IsRegistered => Atom.IsValid || ModuleInstance == HMODULE.Null;

    /// <summary>
    ///  Registers this <see cref="WindowClass"/> so that instances can be created.
    /// </summary>
    public WindowClass Register()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_windowClass is not null && !IsRegistered)
        {
            lock (_lock)
            {
                if (_windowClass is not null && !IsRegistered)
                {
                    Atom = _windowClass.Register();
                }
            }
        }

        return this;
    }

    /// <summary>
    ///  Creates an instance of this <see cref="WindowClass"/>.
    /// </summary>
    /// <param name="bounds">
    ///  Pass <see cref="Window.DefaultBounds"/> for the default size.
    /// </param>
    /// <param name="windowName">
    ///  The text for the title bar when using <see cref="WindowStyles.Caption"/> or <see cref="WindowStyles.Overlapped"/>.
    ///  For buttons, checkboxes, and other static controls this is the text of the control or a resource reference.
    /// </param>
    /// <param name="windowProcedure">
    ///  Optional window procedure to send messages to while attempting to construct the window. Allows for
    ///  <see cref="MessageType.Create"/> to be handled.
    /// </param>
    public virtual HWND CreateWindow(
        Rectangle bounds = default,
        string? windowName = null,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        HWND parentWindow = default,
        nint parameters = default,
        HMENU menuHandle = default,
        WindowProcedure? windowProcedure = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!IsRegistered)
        {
           Register();
        }

        if (bounds == default)
        {
            bounds = new(Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT);
        }

        fixed (char* wn = windowName)
        fixed (char* cn = _className)
        {
            using var themeScope = Application.ThemingScope;
            Application.EnsureDpiAwareness();

            try
            {
                t_initializeProcedure = windowProcedure;

                HWND hwnd = Interop.CreateWindowEx(
                    (WINDOW_EX_STYLE)extendedStyle,
                    Atom.IsValid ? (char*)Atom.Value : cn,
                    wn,
                    (WINDOW_STYLE)style,
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height,
                    parentWindow,
                    menuHandle,
                    HMODULE.Null,
                    (void*)parameters);

                if (hwnd.IsNull)
                {
                    Error.ThrowLastError();
                }

                if (!Atom.IsValid)
                {
                    Atom = (ushort)hwnd.GetClassLong(GET_CLASS_LONG_INDEX.GCW_ATOM);
                }

                return hwnd;
            }
            finally
            {
                t_initializeProcedure = null;
            }
        }
    }

    private LRESULT WindowProcedureInternal(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        if (Disposed)
        {
            // In the middle of disposing, we've flipped the flag, but haven't unregistered the class yet.
            return Interop.DefWindowProc(window, message, wParam, lParam);
        }

        if (t_initializeProcedure is not null
            && (MessageType)message is MessageType.GetMinMaxInfo
                or MessageType.NonClientCreate
                or MessageType.NonClientCalculateSize
                or MessageType.Create)
        {
            // Give the Window a chance to handle the message before it's HWND is fully initialized.
            LRESULT initResult = t_initializeProcedure(window, message, wParam, lParam);

            // These are the observed messages (in order) seen while calling CreateWindow
            //
            //   - WM_GETMINMAXINFO
            //   - WM_NCCREATE
            //   - WM_NCCALCSIZE
            //   - WM_CREATE

            if (initResult != -1)
            {
                return initResult;
            }
        }

        LRESULT result = WindowProcedure(window, (MessageType)message, wParam, lParam);

        // We don't want to be finalized while we're responding to a message.
        GC.KeepAlive(this);
        return result;
    }

    protected virtual LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam) =>
        _priorClassProcedure.IsNull
            ? Interop.DefWindowProc(window, (uint)message, wParam, lParam)
            : Interop.CallWindowProc(_priorClassProcedure, window, (uint)message, wParam, lParam);

    protected override void Dispose(bool disposing)
    {
        if (!Atom.IsValid)
        {
            return;
        }

        // Free the memory for the window class and prevent further callbacks.
        // (Presuming that we don't have to set the default WNDPROC back via SetClassLong, if we do
        //  we can follow along with what Window does.)
        if (Interop.UnregisterClass((char*)Atom.Value, ModuleInstance))
        {
            Atom = default;
        }
        else
        {
            WIN32_ERROR error = Error.GetLastError();
            if (disposing)
            {
                error.Throw();
            }
            else
            {
                // Don't want to throw on the finalizer thread.
                Debug.Fail($"Failed to unregister window class {_className}: \n{error}");
            }
        }
    }
}