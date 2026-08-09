// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Direct2D;

namespace Windows;

/// <summary>
///  Represents a managed wrapper around a native Win32 window.
/// </summary>
public unsafe partial class Window : ComponentBase, IHandle<HWND>, ILayoutHandler
{
    private static readonly object s_lock = new();
    private static readonly ConcurrentDictionary<HWND, WeakReference<Window>> s_windows = new();
    private static readonly WindowClass s_defaultWindowClass = new(className: $"DefaultWindowClass_{Guid.NewGuid()}");

    public static Rectangle DefaultBounds { get; }
        = new(PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT);

    // Default fonts for each DPI
    private static readonly ConcurrentDictionary<int, HFONT> s_defaultFonts = new();
    private static WNDPROC DefaultWindowProcedure { get; } = GetDefaultWindowProcedure();

    // High precision metric units are .01mm each
    private const int HiMetricUnitsPerInch = 2540;

    private readonly object _lock = new();

    // Stash the delegate to keep it from being collected
    private readonly WindowProcedure _windowProcedure;
    private readonly WNDPROC _priorWindowProcedure;
    protected readonly WindowClass _windowClass;

    // Identifies the managed thread that owns the HWND without querying a handle after destruction.
    private readonly Thread _thread = Thread.CurrentThread;

    // Retains the dispatcher affinity after the dispatcher stops and the HWND is destroyed.
    private Threading.Dispatcher? _dispatcher;
    private bool _destroyed;
    private HWND _handle;

    // When I send a WM_GETFONT message to a window, why don't I get a font?
    // https://devblogs.microsoft.com/oldnewthing/20140724-00/?p=413

    // Who is responsible for destroying the font passed in the WM_SETFONT message?
    // https://devblogs.microsoft.com/oldnewthing/20080912-00/?p=20893

    private HFONT _font;
    private HFONT _lastCreatedFont;

    private HwndRenderTarget? _renderTarget;

    protected HwndRenderTarget RenderTarget => _renderTarget ?? throw new InvalidOperationException();

    private uint _lastDpi;

    // PMv2 child messages carry no DPI payload. Preserve the old value before the parent transition so the
    // after-parent notification can report the complete transition even if an ancestor updates this window's font.
    private uint _dpiBeforeParent;
    private Color _backgroundColor;
    private HBRUSH _backgroundBrush;
    private Color _backgroundBrushColor;
    private HBRUSH _controlBackgroundBrush;
    private Color _controlBackgroundBrushColor;
    private int _colorModeGeneration = -1;

    private readonly Features _features;

    [MemberNotNullWhen(true, nameof(_renderTarget))]
    protected bool IsDirect2dEnabled()
    {
        bool enabled = _features.AreFlagsSet(Features.EnableDirect2d);
        if (enabled && _renderTarget is null)
        {
            UpdateRenderTarget(Handle, this.GetClientRectangle().Size);
        }

        return enabled;
    }

    /// <summary>
    ///  The window handle. This will be <see cref="HWND.Null"/> after the window is destroyed.
    /// </summary>
    public HWND Handle => _handle;

    /// <summary>
    ///  Gets the dispatcher associated with the thread that owns this window.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Once associated, the dispatcher remains available from this property after shutdown or handle destruction.
    ///   Queue admission determines whether each operation is accepted; callers do not need to check dispatcher state
    ///   before invoking it. A later <see cref="Application.Run(Window, bool)"/> on the same owning thread associates
    ///   a surviving window with that run's fresh dispatcher after the previous dispatcher completes.
    ///  </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">The window has not been associated with a dispatcher.</exception>
    public Threading.Dispatcher Dispatcher
    {
        get
        {
            Threading.Dispatcher? dispatcher = Volatile.Read(ref _dispatcher);
            if (dispatcher is not null)
            {
                return dispatcher;
            }

            dispatcher = Threading.Dispatcher.FromHandle(this)
                ?? throw new InvalidOperationException("The window has not been associated with a dispatcher.");
            AttachDispatcher(dispatcher);
            return dispatcher;
        }
    }

    object? IHandle<HWND>.Wrapper => this;

    public event WindowsMessageEvent? MessageHandler;

    public Window(
        Rectangle bounds = default,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        Color backgroundColor = default,
        Features features = default)
    {
        _dispatcher = parentWindow?._dispatcher ?? Threading.Dispatcher.Current;
        _windowClass = windowClass ?? s_defaultWindowClass;

        if (bounds.IsEmpty)
        {
            bounds = DefaultBounds;
        }

        _features = features;
        _backgroundColor = backgroundColor;
        UndocumentedDarkMode.ConfigureApplication(Application.CurrentColorState);

        try
        {
            _handle = _windowClass.CreateWindow(
                bounds,
                text,
                style,
                extendedStyle,
                parentWindow?.Handle ?? default,
                parameters,
                menuHandle,
                InitializationWindowProcedure);
        }
        catch
        {
            // Make sure we don't leave a window handle around if we fail to create the window.
            _handle = default;
            throw;
        }

        // Need to set our Window Procedure to get messages before we set
        // the font (which sends a message to do so).
        _windowProcedure = WindowProcedureInternal;

        s_windows[Handle] = new(this);
        _priorWindowProcedure = this.SetWindowProcedure(_windowProcedure);

        _lastDpi = this.GetDpi();
        if (this.GetFontHandle().IsNull)
        {
            // Default system font is applied, use a nicer (ClearType) font
            this.SetFontHandle(GetDefaultFontForDpi((int)_lastDpi));
        }

        ApplyApplicationColorMode(invokeCallback: false);
    }

    /// <summary>
    ///  Associates this window with its owning thread's dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to associate with the window.</param>
    internal void AttachDispatcher(Threading.Dispatcher dispatcher)
    {
        while (true)
        {
            Threading.Dispatcher? existing = Volatile.Read(ref _dispatcher);
            if (ReferenceEquals(existing, dispatcher))
            {
                return;
            }

            if (existing is not null && !existing.Completion.IsCompleted)
            {
                throw new InvalidOperationException("The window is already associated with another active dispatcher.");
            }

            if (ReferenceEquals(Interlocked.CompareExchange(ref _dispatcher, dispatcher, existing), existing))
            {
                return;
            }
        }
    }

    /// <summary>
    ///  Associates existing managed windows owned by the current thread with its dispatcher.
    /// </summary>
    /// <param name="dispatcher">The current thread's dispatcher.</param>
    internal static void AttachDispatcherToCurrentThread(Threading.Dispatcher dispatcher)
    {
        dispatcher.VerifyAccess();

        foreach (WeakReference<Window> reference in s_windows.Values)
        {
            if (reference.TryGetTarget(out Window? window) && ReferenceEquals(window._thread, Thread.CurrentThread))
            {
                window.AttachDispatcher(dispatcher);
            }
        }
    }

    internal static void ApplyApplicationColorModeToWindows()
    {
        HashSet<Threading.Dispatcher> postedDispatchers = [];
        foreach (WeakReference<Window> reference in s_windows.Values)
        {
            if (!reference.TryGetTarget(out Window? window) || window.Handle.IsNull)
            {
                continue;
            }

            if (ReferenceEquals(window._thread, Thread.CurrentThread))
            {
                window.ApplyApplicationColorMode(invokeCallback: true);
            }
            else if (window._dispatcher is { } dispatcher && postedDispatchers.Add(dispatcher))
            {
                _ = dispatcher.TryPost(ApplyApplicationColorModeToCurrentThread);
            }
        }
    }

    private static void ApplyApplicationColorModeToCurrentThread()
    {
        foreach (WeakReference<Window> reference in s_windows.Values)
        {
            if (reference.TryGetTarget(out Window? window)
                && !window.Handle.IsNull
                && ReferenceEquals(window._thread, Thread.CurrentThread))
            {
                window.ApplyApplicationColorMode(invokeCallback: true);
            }
        }
    }

    private static HFONT GetDefaultFontForDpi(int dpi)
    {
        if (!s_defaultFonts.TryGetValue(dpi, out HFONT font))
        {
            lock (s_lock)
            {
                if (!s_defaultFonts.TryGetValue(dpi, out font))
                {
                    font = HFONT.CreateFont(
                        typeface: "Microsoft Sans Serif",
                        height: HFONT.GetHeightForDpi(pointSize: 12, dpi),
                        quality: FontQuality.ClearTypeNatural);

                    s_defaultFonts[dpi] = font;
                }
            }
        }

        return font;
    }

    public void SetFont(string typeFace, int pointSize)
    {
        HFONT newFont = HFONT.CreateFont(
            typeface: typeFace,
            height: HFONT.GetHeightForDpi(pointSize, (int)this.GetDpi()),
            quality: FontQuality.ClearTypeNatural);

        if (!_lastCreatedFont.IsNull)
        {
            _lastCreatedFont.Dispose();
        }

        _lastCreatedFont = newFont;

        this.SetFontHandle(_lastCreatedFont);
    }

    private void UpdateRenderTarget(HWND window, Size size)
    {
        if (_renderTarget is null)
        {
            _renderTarget = HwndRenderTarget.CreateForWindow(Application.Direct2dFactory, window, size);
            RenderTargetCreated();
        }
        else
        {
            _renderTarget.Resize(size);
        }
    }

    /// <summary>
    ///  Called whenever the Direct2D render target has been created or recreated.
    /// </summary>
    protected virtual void RenderTargetCreated()
    {
    }

    protected virtual void OnPaint()
    {
    }

    protected virtual void OnSize(Size size)
    {
    }

    /// <summary>Called after this window has transitioned to a different DPI.</summary>
    /// <param name="oldDpi">The effective DPI before the transition.</param>
    /// <param name="newDpi">The effective DPI after the transition.</param>
    /// <remarks>
    ///  <para>
    ///   For a top-level window, the suggested bounds have been applied before this method is called. For a child
    ///   window under a Per-Monitor V2 top-level window, this method is called while processing
    ///   <see cref="MessageType.DpiChangedAfterParent"/>. Layout coordinates and HWND bounds are physical pixels.
    ///  </para>
    /// </remarks>
    protected virtual void OnDpiChanged(uint oldDpi, uint newDpi)
    {
    }

    /// <summary>
    ///  Called whenever a command is sent to the window.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Control classes send the command to the parent Window first. We also reflect the message back to
    ///   the control Window so that it can handle the message. This is similar to MFC/WinForms behavior.
    ///  </para>
    /// </remarks>
    protected virtual void OnCommand(int controlId, int notificationCode)
    {
    }

    private LRESULT InitializationWindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        if (Handle.IsNull)
        {
            // In the middle of CreateWindow, set our handle so that the "this" pointer is valid for use.
            // This enables things such as parenting children during WM_CREATE.

            _handle = window;
        }

        return WindowProcedureInternal(window, message, wParam, lParam);
    }

    private LRESULT WindowProcedureInternal(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        // What is the difference between WM_DESTROY and WM_NCDESTROY?
        // https://devblogs.microsoft.com/oldnewthing/20050726-00/?p=34803

        // Check for messages that we need to process before invoking handlers. Currently this means making
        // sure that Direct2D is in the right state if it has been opted into.
        switch (message)
        {
            case Interop.WM_SIZE:
                Size size = new(lParam.LOWORD, lParam.HIWORD);

                // Check the flag directly here so we don't create then resize.
                if (_features.AreFlagsSet(Features.EnableDirect2d))
                {
                    UpdateRenderTarget(window, size);
                }

                break;

            case Interop.WM_PAINT:
                if (IsDirect2dEnabled())
                {
                    _renderTarget.BeginDraw();
                    _renderTarget.SetTransform(Matrix3x2.Identity);
                    Window backgroundOwner = GetBackgroundOwner();
                    _renderTarget.Clear(backgroundOwner.GetEffectiveBackgroundColor(controlSurface: false));
                }

                break;
        }

        // Let attached handlers have a chance to deal with the message.
        bool handled = InvokeHandlers(out LRESULT result);

        // Handle messages that we need to update state or invoke virtuals on.
        switch (message)
        {
            case Interop.WM_NCDESTROY:
                lock (_lock)
                {
                    // This should be the final message. Track that we've been destroyed so we know we don't have
                    // to manually clean up.

                    bool success = s_windows.TryRemove(Handle, out _);
                    Debug.Assert(success);
                    _handle = default;
                    _destroyed = true;
                }

                break;

            case Interop.WM_SIZE:
                Size size = new(lParam.LOWORD, lParam.HIWORD);
                OnSize(size);
                break;

            case Interop.WM_PAINT:
                OnPaint();
                break;
        }

        if (!handled)
        {
            // Not marked as handled, call the virtual method to allow for "normal" processing.
            result = WindowProcedure(window, (MessageType)message, wParam, lParam);
        }

        if (message == PInvoke.WM_PAINT && IsDirect2dEnabled())
        {
            _renderTarget.EndDraw(out bool recreateTarget);
            if (recreateTarget)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
                UpdateRenderTarget(window, this.GetClientRectangle().Size);
            }
        }

        // Ensure we're not collected while we're processing a message.
        GC.KeepAlive(this);
        return result;

        bool InvokeHandlers(out LRESULT result)
        {
            if (MessageHandler is { } handlers)
            {
                foreach (var handler in handlers.GetInvocationList().OfType<WindowsMessageEvent>())
                {
                    LRESULT? handlerResult = handler(this, window, (MessageType)message, wParam, lParam);
                    if (handlerResult.HasValue)
                    {
                        result = handlerResult.Value;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    ///  Override to handle window messages. Call base to allow default handling.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Note that some messages will be sent before the class constructor has fully run. These messages are
    ///   <see cref="MessageType.GetMinMaxInfo"/>, <see cref="MessageType.NonClientCreate"/>,
    ///   <see cref="MessageType.NonClientCalculateSize"/> and <see cref="MessageType.Create"/>. Do not access
    ///  </para>
    /// </remarks>
    protected virtual LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            // https://learn.microsoft.com/windows/win32/gdi/window-background
            // https://learn.microsoft.com/windows/win32/gdi/drawing-a-custom-window-background
            case MessageType.EraseBackground:

                if (IsDirect2dEnabled())
                {
                    // Having the HDC erased will cause flicker, so say we handled it. We could erase using
                    // Direct2D, but pushing that to the paint method is avoids an extra BeginDraw/EndDraw.
                    return (LRESULT)1;
                }
                else if (GetBackgroundOwner() is { } backgroundOwner)
                {
                    ((HDC)wParam).FillRectangle(this.GetClientRectangle(), backgroundOwner.GetBackgroundBrush());
                    return (LRESULT)1;
                }

                break;

            case MessageType.ControlColorMessageBox:
            case MessageType.ControlColorEdit:
            case MessageType.ControlColorListBox:
            case MessageType.ControlColorButton:
            case MessageType.ControlColorDialog:
            case MessageType.ControlColorScrollBar:
            case MessageType.ControlColorStatic:
                Window control = lParam == 0
                    ? this
                    : FromHandle((HWND)lParam, walkParents: true) ?? this;
                Window controlBackgroundOwner = control.GetBackgroundOwner();
                bool controlSurface = message is MessageType.ControlColorEdit
                    or MessageType.ControlColorListBox
                    or MessageType.ControlColorScrollBar;
                DeviceContext controlContext = (DeviceContext)wParam;
                controlContext.SetBackgroundColor(controlBackgroundOwner.GetEffectiveBackgroundColor(controlSurface));
                controlContext.SetTextColor(control.GetEffectiveForegroundColor(controlSurface));
                return (LRESULT)controlBackgroundOwner.GetBackgroundBrush(controlSurface).Value;

            case MessageType.GetFont:
                // We only want to handle fonts if we're not an externally registered class.
                if (!_windowClass.IsSubclassed)
                {
                    return (LRESULT)_font.Value;
                }

                break;

            case MessageType.SetFont:
                if (!_windowClass.IsSubclassed)
                {
                    _font = (HFONT)(nint)wParam.Value;
                    if ((BOOL)lParam.LOWORD)
                    {
                        this.Invalidate();
                    }

                    return (LRESULT)0;
                }

                break;

            case MessageType.DpiChanged:
                if (lParam != 0)
                {
                    HandleDpiChanged(new(wParam, lParam));
                }

                return ForwardDpiMessageToRegisteredClass(window, message, wParam, lParam);

            case MessageType.DpiChangedBeforeParent:
                _dpiBeforeParent = _lastDpi;
                return ForwardDpiMessageToRegisteredClass(window, message, wParam, lParam);

            case MessageType.DpiChangedAfterParent:
                HandleDpiChangedAfterParent();
                return ForwardDpiMessageToRegisteredClass(window, message, wParam, lParam);

            case MessageType.SettingChange:
            case MessageType.SystemColorChange:
            case MessageType.ThemeChanged:
                Application.RefreshSystemColorMode();
                break;

            case MessageType.Command:
                if (lParam != 0 && FromHandle((HWND)lParam, walkParents: false) is Window child)
                {
                    // Control command from a child control, reflect the message to the control Window.
                    // (Matching MFC/WinForms behavior here.)
                    LRESULT result = child.SendMessage(MessageType.ReflectCommand, wParam, lParam);
                    OnCommand(wParam.LOWORD, wParam.HIWORD);
                    return result;
                }

                break;

            case MessageType.ReflectCommand:
                OnCommand(wParam.LOWORD, wParam.HIWORD);

                // 0 means we handled the Command, no reason to call base as this message was reflected to us.
                return (LRESULT)0;
        }

        return _priorWindowProcedure.IsNull
            // Still creating the window.
            ? (LRESULT)(-1)
            : PInvoke.CallWindowProc(_priorWindowProcedure, window, (uint)message, wParam, lParam);
    }

    /// <summary>Called after the effective application color state changes.</summary>
    /// <remarks>
    ///  <para>
    ///   This method is called on the window's owning UI thread after <see cref="Application.CurrentColorState"/> is
    ///   updated. Derived controls should recreate palette-dependent resources here and then call the base method.
    ///   Initial construction does not invoke this virtual method; initialize those resources in the derived
    ///   constructor or the relevant creation hook as well.
    ///  </para>
    /// </remarks>
    protected virtual void OnColorModeChanged()
    {
    }

    /// <summary>Applies the current application color state to this window using a private dark theme class.</summary>
    /// <param name="darkThemeName">The private visual-style class name used when dark mode is active.</param>
    /// <remarks>
    ///  <para>
    ///   Call this after the window handle is created and again from <see cref="OnColorModeChanged"/>. The framework
    ///   applies the private theme only when <see cref="Application.UseUndocumentedDarkModeApis"/> is enabled, Dark
    ///   mode is resolved, and High Contrast is inactive. Otherwise, it removes the prior private association.
    ///  </para>
    /// </remarks>
    protected void ApplyApplicationDarkModeTheme(string darkThemeName)
        => ApplyApplicationDarkModeTheme(Handle, darkThemeName);

    /// <summary>Applies the current application color state using private dark visual-style identifiers.</summary>
    /// <param name="darkSubAppName">The private sub-app name used when dark mode is active, or <see langword="null"/>.</param>
    /// <param name="darkSubIdList">The private sub-ID list used when dark mode is active, or <see langword="null"/>.</param>
    /// <remarks>
    ///  <para>
    ///   Call this after the window handle is created and again from <see cref="OnColorModeChanged"/>. At least one
    ///   identifier must be supplied. The framework removes both identifiers when private dark theming is inactive.
    ///  </para>
    /// </remarks>
    protected void ApplyApplicationDarkModeTheme(string? darkSubAppName, string? darkSubIdList)
        => ApplyApplicationDarkModeTheme(Handle, darkSubAppName, darkSubIdList);

    /// <summary>Applies the current application color state to an owned native window using a private dark theme class.</summary>
    /// <param name="window">The owned native window whose visual-style association is updated.</param>
    /// <param name="darkThemeName">The private visual-style class name used when dark mode is active.</param>
    /// <remarks>
    ///  <para>
    ///   This overload supports unwrapped child or popup windows owned by a derived control. The supplied handle must
    ///   remain valid for the duration of the call.
    ///  </para>
    /// </remarks>
    protected void ApplyApplicationDarkModeTheme(HWND window, string darkThemeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(darkThemeName);
        ApplyApplicationDarkModeTheme(window, darkThemeName, darkSubIdList: null);
    }

    /// <summary>Applies private dark visual-style identifiers to an owned native window.</summary>
    /// <param name="window">The owned native window whose visual-style association is updated.</param>
    /// <param name="darkSubAppName">The private sub-app name used when dark mode is active, or <see langword="null"/>.</param>
    /// <param name="darkSubIdList">The private sub-ID list used when dark mode is active, or <see langword="null"/>.</param>
    /// <remarks>
    ///  <para>
    ///   This overload supports controls that select a private theme through the sub-ID list and unwrapped child or
    ///   popup windows owned by a derived control. The supplied handle must remain valid for the duration of the call.
    ///  </para>
    /// </remarks>
    protected void ApplyApplicationDarkModeTheme(HWND window, string? darkSubAppName, string? darkSubIdList)
    {
        if (darkSubAppName is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(darkSubAppName);
        }

        if (darkSubIdList is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(darkSubIdList);
        }

        if (darkSubAppName is null && darkSubIdList is null)
        {
            throw new ArgumentException("A dark theme sub-app name or sub-ID list is required.");
        }

        UndocumentedDarkMode.ApplyWindowTheme(
            window,
            Application.CurrentColorState,
            darkSubAppName,
            darkSubIdList);
    }

    private Window GetBackgroundOwner()
    {
        Window current = this;
        while (current._backgroundColor.IsEmpty && !current.Handle.IsNull)
        {
            HWND parentHandle = PInvoke.GetParent(current.Handle);
            Window? parent = parentHandle.IsNull ? null : FromHandle(parentHandle, walkParents: true);
            if (parent is null || ReferenceEquals(parent, current))
            {
                break;
            }

            current = parent;
        }

        return current;
    }

    /// <summary>Gets the effective inherited background color for this window.</summary>
    /// <param name="controlSurface">
    ///  <see langword="true"/> to use the semantic interactive-control background when no explicit background is
    ///  inherited; <see langword="false"/> to use the semantic window background.
    /// </param>
    /// <returns>The nearest explicit ancestor background or the current semantic default.</returns>
    protected Color GetEffectiveBackgroundColor(bool controlSurface = false)
    {
        Window backgroundOwner = GetBackgroundOwner();
        if (!backgroundOwner._backgroundColor.IsEmpty)
        {
            return backgroundOwner._backgroundColor;
        }

        ApplicationColorPalette palette = Application.CurrentColorState.Palette;
        return controlSurface ? palette.ControlBackground : palette.WindowBackground;
    }

    /// <summary>Gets the effective enabled or disabled semantic foreground color for this window.</summary>
    /// <param name="controlSurface">
    ///  <see langword="true"/> to use the semantic interactive-control foreground; <see langword="false"/> to use
    ///  the semantic window foreground.
    /// </param>
    /// <returns>The current enabled foreground, or the disabled foreground when this window is disabled.</returns>
    protected Color GetEffectiveForegroundColor(bool controlSurface = true)
    {
        ApplicationColorPalette palette = Application.CurrentColorState.Palette;
        if (!PInvoke.IsWindowEnabled(Handle))
        {
            return palette.DisabledForeground;
        }

        return controlSurface ? palette.ControlForeground : palette.WindowForeground;
    }

    private HBRUSH GetBackgroundBrush(bool controlSurface = false)
    {
        Color color = GetEffectiveBackgroundColor(controlSurface);
        ref HBRUSH brush = ref controlSurface ? ref _controlBackgroundBrush : ref _backgroundBrush;
        ref Color brushColor = ref controlSurface ? ref _controlBackgroundBrushColor : ref _backgroundBrushColor;
        if (brush.IsNull || brushColor != color)
        {
            brush.Dispose();
            brush = HBRUSH.CreateSolid(color);
            brushColor = color;
        }

        return brush;
    }

    private void ApplyApplicationColorMode(bool invokeCallback)
    {
        if (Handle.IsNull)
        {
            return;
        }

        ApplicationColorState state = Application.CurrentColorState;
        if (_colorModeGeneration == state.Generation)
        {
            return;
        }

        _colorModeGeneration = state.Generation;
        _backgroundBrush.Dispose();
        _backgroundBrush = default;
        _backgroundBrushColor = default;
        _controlBackgroundBrush.Dispose();
        _controlBackgroundBrush = default;
        _controlBackgroundBrushColor = default;
        UndocumentedDarkMode.ConfigureApplication(state);
        ApplyTitleBarColorMode(state);
        if (invokeCallback)
        {
            OnColorModeChanged();
        }

        this.Invalidate(erase: true);
    }

    private void ApplyTitleBarColorMode(ApplicationColorState state)
    {
        if (PInvoke.GetAncestor(Handle, GET_ANCESTOR_FLAGS.GA_ROOT) != Handle)
        {
            return;
        }

        BOOL dark = state.IsDark && !state.IsHighContrast;
        // These attributes are unavailable on older Windows versions. Client-area theming remains functional when
        // DWM rejects an attribute, so compatibility failures are intentionally nonfatal.
        _ = PInvoke.DwmSetWindowAttribute(
            Handle,
            DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
            &dark,
            (uint)sizeof(BOOL));

        uint caption = state.IsHighContrast
            ? uint.MaxValue
            : ((COLORREF)state.Palette.WindowBackground).Value;
        uint text = state.IsHighContrast
            ? uint.MaxValue
            : ((COLORREF)state.Palette.WindowForeground).Value;
        uint border = state.IsHighContrast
            ? uint.MaxValue
            : ((COLORREF)state.Palette.Border).Value;
        _ = PInvoke.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, &caption, sizeof(uint));
        _ = PInvoke.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_TEXT_COLOR, &text, sizeof(uint));
        _ = PInvoke.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, &border, sizeof(uint));
    }

    private void HandleDpiChanged(Message.DpiChanged dpiChanged)
    {
        uint oldDpi = _lastDpi;
        uint newDpi = dpiChanged.Dpi;
        UpdateFontForDpi(oldDpi, newDpi);
        UpdateDescendantFontsForDpi();
        this.MoveWindow(dpiChanged.SuggestedBounds, repaint: true);
        if (oldDpi != 0 && oldDpi != newDpi)
        {
            OnDpiChanged(oldDpi, newDpi);
        }
    }

    private LRESULT ForwardDpiMessageToRegisteredClass(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        // Wrapped system controls retain DPI-specific behavior in their original class procedure. Framework-owned
        // classes have fully processed the message here and follow the documented zero-result contract.
        return _windowClass.IsSubclassed && !_priorWindowProcedure.IsNull
            ? PInvoke.CallWindowProc(_priorWindowProcedure, window, (uint)message, wParam, lParam)
            : default;
    }

    private void HandleDpiChangedAfterParent()
    {
        // PMv2 sends this to child HWNDs after the top-level WM_DPICHANGED, but supplies no new DPI or suggested
        // bounds. Sample the child's effective DPI now that the parent transition is complete.
        uint oldDpi = _dpiBeforeParent == 0 ? _lastDpi : _dpiBeforeParent;
        uint newDpi = this.GetDpi();
        _dpiBeforeParent = 0;

        if (_lastDpi != newDpi)
        {
            UpdateFontForDpi(_lastDpi, newDpi);
        }

        if (oldDpi != 0 && oldDpi != newDpi)
        {
            OnDpiChanged(oldDpi, newDpi);
        }
    }

    private void UpdateFontForDpi(uint lastDpi, uint newDpi)
    {
        if (newDpi == 0 || lastDpi == newDpi)
        {
            return;
        }

        if (lastDpi == 0)
        {
            _lastDpi = newDpi;
            return;
        }

        HFONT currentFont = this.GetFontHandle();
        HFONT lastCreatedFont = _lastCreatedFont;

        // Check to see if we're using one of our managed fonts.

        if (!lastCreatedFont.IsNull && lastCreatedFont == currentFont)
        {
            // One that we created that isn't a static default
            var logfont = currentFont.GetLogicalFont();
            float scale = (float)newDpi / lastDpi;
            logfont.lfHeight = (int)(logfont.lfHeight * scale);
            HFONT newFont = PInvoke.CreateFontIndirect(&logfont);
            this.SetFontHandle(newFont);
            _lastCreatedFont = newFont;
            lastCreatedFont.Dispose();
        }
        else if (GetDefaultFontForDpi((int)lastDpi) == currentFont)
        {
            // Was our default font, use the new scale
            this.SetFontHandle(GetDefaultFontForDpi((int)newDpi));
        }

        _lastDpi = newDpi;
    }

    private void UpdateDescendantFontsForDpi()
    {
        // EnumChildWindows already walks the entire descendant tree. Update each managed HWND once rather than
        // recursing from the callback and revisiting grandchildren.
        this.EnumerateChildWindows(child =>
        {
            if (FromHandle(child) is { } childWindow)
            {
                childWindow.UpdateFontForDpi(childWindow._lastDpi, childWindow.GetDpi());
            }

            return true;
        });
    }

    /// <summary>
    ///  Try to get the <see cref="Window"/> from the given <paramref name="handle"/>. Walks parent windows
    ///  if there is no matching <see cref="Window"/> and <paramref name="walkParents"/> is <see langword="true"/>.
    /// </summary>
    public static Window? FromHandle<T>(T handle, bool walkParents = false)
        where T : IHandle<HWND>
    {
        if (handle is null || handle.Handle.IsNull)
        {
            return null;
        }

        if (handle is Window window)
        {
            return window;
        }

        HWND hwnd = handle.Handle;
        if (s_windows.TryGetValue(hwnd, out var weakReference))
        {
            if (weakReference.TryGetTarget(out Window? found))
            {
                return found;
            }
            else
            {
                Debug.Fail("Dead weak ref. Window.Dispose not called.");
            }
        }

        if (!walkParents)
        {
            return null;
        }

        hwnd = PInvoke.GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_PARENT);
        return hwnd.IsNull ? null : FromHandle(hwnd, walkParents: true);
    }

    /// <remarks>
    ///  <para>
    ///   Note that the <see cref="Handle"/> may be <see cref="HWND.Null"/> when this method is called. When the
    ///   underlying <see cref="HWND"/> is destroyed, the handle is no longer valid and will be set to null.
    ///  </para>
    /// </remarks>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        // We want to block at a WM_NCDESTROY message so that we know our handle is still valid for cleanup.
        lock (_lock)
        {
            if (!_destroyed)
            {
                // Set back the default Window procedure as we don't want any messages coming in anymore.
                Handle.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)(void*)DefaultWindowProcedure);

                // Send a close message to the window. This will cause the window to be destroyed. If we're being
                // finalized, post instead of send to ensure the message is processed on the right thread.
                if (!disposing)
                {
                    Handle.PostMessage(MessageType.Close);
                }
                else
                {
                    HWND handle = Handle;
                    handle.SendMessage(MessageType.Close);
                    bool success = s_windows.TryRemove(handle, out _);
                    Debug.Assert(success);
                    _handle = default;
                    _destroyed = true;
                }
            }
        }

        if (disposing)
        {
            _backgroundBrush.Dispose();
            _controlBackgroundBrush.Dispose();
            _lastCreatedFont.Dispose();
            _font.Dispose();
            _renderTarget?.Dispose();
        }
    }

    void ILayoutHandler.Layout(Rectangle bounds, float scale) => LayoutWindow(bounds);

    protected virtual void LayoutWindow(Rectangle bounds)
    {
        if (bounds != this.GetClientRectangle())
        {
            Handle.MoveWindow(bounds, repaint: true);
        }
    }

    public static implicit operator HWND(Window window) => window.Handle;

    /// <summary>
    ///  Allows preprocessing messages before they are translated and dispatched.
    /// </summary>
    /// <returns><see langword="true"/> if handled and translation and dispatching should be skipped.</returns>
    protected internal virtual bool PreProcessMessage(ref MSG message) => false;

    public int PixelToHiMetric(int pixels)
        => (int)(((HiMetricUnitsPerInch * pixels) + (_lastDpi >> 1)) / _lastDpi);

    public Size PixelToHiMetric(Size size)
        => new(PixelToHiMetric(size.Width), PixelToHiMetric(size.Height));

    public int HiMetricToPixel(int units)
        => (int)(((_lastDpi * units) + (HiMetricUnitsPerInch / 2)) / HiMetricUnitsPerInch);

    private static WNDPROC GetDefaultWindowProcedure()
    {
        HMODULE module = PInvoke.LoadLibrary("user32.dll");
        Debug.Assert(!module.IsNull);
        FARPROC address = PInvoke.GetProcAddress(module, "DefWindowProcW");
        Debug.Assert(!address.IsNull);
        return (WNDPROC)(void*)address.Value;
    }
}