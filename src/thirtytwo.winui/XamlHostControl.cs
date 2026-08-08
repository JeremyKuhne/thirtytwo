// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Windows.Graphics;
using Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Windows.WinUI;

/// <summary>
///  Hosts WinUI content in a managed thirtytwo child window.
/// </summary>
/// <remarks>
///  <para>
///   Construction must occur on the designated XAML UI thread inside an active
///   <see cref="Application.Run(Window, bool)"/> loop. The host acquires its own
///   <see cref="XamlHostEnvironment"/> lease, creates one <see cref="DesktopWindowXamlSource"/>, and attaches the
///   source only after the <see cref="CustomControl"/> constructor returns.
///  </para>
///  <para>
///   The host owns the XAML source but not the assigned content. Disposal clears <see cref="Content"/>, disposes the
///   source, and releases the environment lease before destroying the managed host window. Parent destruction performs
///   the same cleanup. A host not otherwise destroyed is retained for owner-thread cleanup during dispatcher shutdown.
///  </para>
/// </remarks>
public unsafe partial class XamlHostControl : CustomControl
{
    private static readonly WindowClass s_windowClass = new(
        className: "ThirtyTwoXamlHostControl",
        backgroundBrush: HBRUSH.Invalid);

    private readonly XamlThreadAffinity _affinity = new();
    private XamlHostContext? _context;
    private XamlHostEnvironment? _environment;
    private DesktopWindowXamlSource? _xamlSource;
    private ShutdownRegistration _shutdownRegistration;
    private ElementTheme? _applicationRequestedTheme;
    private Guid _reportedFocusRequestId;
    private bool _xamlStateDisposed;

    /// <summary>
    ///  Creates an empty WinUI host attached to <paramref name="parentWindow"/>.
    /// </summary>
    /// <param name="bounds">The host bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The managed parent window.</param>
    public XamlHostControl(Rectangle bounds, Window parentWindow)
        : base(
            bounds,
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop
                | WindowStyles.ClipChildren | WindowStyles.ClipSiblings,
            parentWindow: ValidateParent(parentWindow),
            windowClass: s_windowClass)
    {
        try
        {
            _environment = XamlHostEnvironment.Acquire();
            _context = new(_environment);
            _xamlSource = CreateXamlSource();
            _shutdownRegistration = Dispatcher.RegisterShutdownCallback(Dispose);
        }
        catch (Exception constructionFailure)
        {
            ThrowAfterFailedConstruction(constructionFailure);
        }
    }

    /// <summary>
    ///  Creates a WinUI host and assigns content produced after the host environment and XAML source are initialized.
    /// </summary>
    /// <param name="bounds">The host bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The managed parent window.</param>
    /// <param name="contentFactory">Creates the content on the host thread.</param>
    public XamlHostControl(Rectangle bounds, Window parentWindow, Func<UIElement> contentFactory)
        : this(bounds, parentWindow)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(contentFactory);
            Content = contentFactory()
                ?? throw new InvalidOperationException("The WinUI content factory returned null.");
        }
        catch (Exception constructionFailure)
        {
            ThrowAfterFailedConstruction(constructionFailure);
        }
    }

    /// <summary>
    ///  Creates a WinUI host and passes its non-disposable context to the content factory after initialization.
    /// </summary>
    /// <param name="bounds">The host bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The managed parent window.</param>
    /// <param name="contentFactory">Creates content using services owned by this host.</param>
    public XamlHostControl(
        Rectangle bounds,
        Window parentWindow,
        Func<XamlHostContext, UIElement> contentFactory)
        : this(bounds, parentWindow)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(contentFactory);
            Content = contentFactory(Context)
                ?? throw new InvalidOperationException("The WinUI content factory returned null.");
        }
        catch (Exception constructionFailure)
        {
            ThrowAfterFailedConstruction(constructionFailure);
        }
    }

    /// <summary>Gets WinUI services backed by the environment lease owned by this host.</summary>
    /// <exception cref="ObjectDisposedException">The host or its parent window has been destroyed.</exception>
    public XamlHostContext Context
    {
        get
        {
            _ = GetXamlSource();
            return _context ?? throw new ObjectDisposedException(nameof(XamlHostControl));
        }
    }

    /// <summary>Gets or sets the WinUI element displayed by this host.</summary>
    /// <remarks>
    ///  <para>
    ///   Assignment and access must occur on the owner thread. Replacing or clearing content does not dispose the
    ///   previously assigned element.
    ///  </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">The calling thread does not own this host.</exception>
    /// <exception cref="ObjectDisposedException">The host or its parent window has been destroyed.</exception>
    public virtual UIElement? Content
    {
        get
        {
            DesktopWindowXamlSource xamlSource = GetXamlSource();
            return xamlSource.Content;
        }
        set
        {
            DesktopWindowXamlSource xamlSource = GetXamlSource();
            if (!ReferenceEquals(xamlSource.Content, value))
            {
                _applicationRequestedTheme = null;
            }

            xamlSource.Content = value;
            ApplyApplicationTheme(value);
        }
    }

    /// <summary>Occurs when focus enters the hosted XAML content.</summary>
    public event EventHandler? XamlGotFocus;

    /// <summary>Changes the managed host window's parent and reattaches its content through a new XAML source.</summary>
    /// <param name="parentWindow">The new parent window on the host's owner thread.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parentWindow"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    ///  The calling thread does not own the host, the parent has been destroyed, or the parent belongs to another
    ///  native thread.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The host has been disposed.</exception>
    /// <exception cref="Win32Exception">The native parent could not be changed.</exception>
    /// <exception cref="AggregateException">
    ///  Reparenting failed and the original parent and XAML source could not be restored. The host is disposed before
    ///  the exception is thrown.
    /// </exception>
    public void Reparent(Window parentWindow)
    {
        ArgumentNullException.ThrowIfNull(parentWindow);
        _ = GetXamlSource();

        uint parentThreadId = PInvoke.GetWindowThreadProcessId(parentWindow.Handle, null);
        if (parentThreadId == 0)
        {
            throw new InvalidOperationException("The new parent window has been destroyed.");
        }

        if (parentThreadId != _affinity.NativeThreadId)
        {
            throw new InvalidOperationException(
                $"The new parent window belongs to native thread {parentThreadId}; expected native thread {_affinity.NativeThreadId}.");
        }

        HWND originalParent = PInvoke.GetParent(Handle);
        if (originalParent == parentWindow.Handle)
        {
            return;
        }

        UIElement? content = null;

        try
        {
            DetachXamlSource(out content);
            SetNativeParent(Handle, parentWindow.Handle);
            _xamlSource = CreateXamlSource(content);
        }
        catch (Exception reparentFailure)
        {
            try
            {
                if (!originalParent.IsNull && PInvoke.GetParent(Handle) != originalParent)
                {
                    SetNativeParent(Handle, originalParent);
                }

                _xamlSource ??= CreateXamlSource(content);
            }
            catch (Exception recoveryFailure)
            {
                List<Exception> failures = [reparentFailure, recoveryFailure];
                try
                {
                    DisposeXamlState();
                }
                catch (Exception cleanupFailure)
                {
                    failures.Add(cleanupFailure);
                }

                try
                {
                    base.Dispose(disposing: true);
                }
                catch (Exception windowFailure)
                {
                    failures.Add(windowFailure);
                }

                throw new AggregateException(
                    "Reparenting failed and the original XAML host state could not be restored.",
                    failures);
            }

            throw;
        }
        finally
        {
            GC.KeepAlive(parentWindow);
        }
    }

    /// <summary>Verifies that the calling thread owns this host.</summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own this host.</exception>
    protected void VerifyAccess() => _affinity.VerifyAccess();

    /// <summary>Gets whether the XAML source has been disposed.</summary>
    protected bool IsXamlSourceDisposed => _xamlStateDisposed;

    /// <inheritdoc/>
    protected override void OnSize(Size size)
    {
        DesktopWindowXamlSource? xamlSource = _xamlSource;
        if (xamlSource is not null)
        {
            try
            {
                ResizeSiteBridge(xamlSource, size);
            }
            catch (Exception exception)
            {
                ReportNativeCallbackFailure("Resize", exception);
            }
        }

        base.OnSize(size);
    }

    /// <inheritdoc/>
    protected override void OnColorModeChanged()
    {
        ApplyApplicationTheme(_xamlSource?.Content);
        base.OnColorModeChanged();
    }

    /// <inheritdoc/>
    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.SetFocus:
                NavigateIntoXaml();
                return (LRESULT)0;
            case MessageType.Destroy:
                try
                {
                    DisposeXamlState();
                }
                catch (Exception exception)
                {
                    ReportNativeCallbackFailure("Destroy", exception);
                }

                break;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing: false);
            return;
        }

        VerifyAccess();
        Exception? cleanupFailure = null;
        try
        {
            DisposeXamlState();
        }
        catch (Exception exception)
        {
            cleanupFailure = exception;
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception windowFailure) when (cleanupFailure is not null)
        {
            throw new AggregateException(cleanupFailure, windowFailure);
        }

        if (cleanupFailure is not null)
        {
            ExceptionDispatchInfo.Capture(cleanupFailure).Throw();
        }
    }

    /// <summary>Reports a failure caught at a native window-procedure boundary.</summary>
    protected static void ReportNativeCallbackFailure(string operation, Exception exception)
    {
        try
        {
            XamlHostEventSource.Log.HostCallbackFailed(
                PInvoke.GetCurrentThreadId(),
                operation,
                exception.HResult,
                exception.GetType().FullName ?? exception.GetType().Name);
        }
        catch
        {
        }
    }

    private static Window ValidateParent(Window? parentWindow)
    {
        ArgumentNullException.ThrowIfNull(parentWindow);
        return parentWindow;
    }

    private static void ResizeSiteBridge(DesktopWindowXamlSource xamlSource, Size size)
        => xamlSource.SiteBridge?.MoveAndResize(new RectInt32(0, 0, size.Width, size.Height));

    private static void SetNativeParent(HWND window, HWND parent)
    {
        Marshal.SetLastPInvokeError(0);
        HWND previousParent = PInvoke.SetParent(window, parent);
        int error = Marshal.GetLastPInvokeError();
        if (previousParent.IsNull && error != 0)
        {
            throw new Win32Exception(error);
        }
    }

    [DoesNotReturn]
    private void ThrowAfterFailedConstruction(Exception constructionFailure)
    {
        List<Exception>? cleanupFailures = null;
        try
        {
            DisposeXamlState();
        }
        catch (Exception cleanupFailure)
        {
            cleanupFailures = [cleanupFailure];
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception windowFailure)
        {
            (cleanupFailures ??= []).Add(windowFailure);
        }

        if (cleanupFailures is not null)
        {
            cleanupFailures.Insert(0, constructionFailure);
            throw new AggregateException("XAML host construction and cleanup failed.", cleanupFailures);
        }

        ExceptionDispatchInfo.Capture(constructionFailure).Throw();
        throw new UnreachableException();
    }

    private DesktopWindowXamlSource CreateXamlSource(UIElement? content = null)
    {
        DesktopWindowXamlSource xamlSource = new();
        try
        {
            xamlSource.Initialize(Win32Interop.GetWindowIdFromWindow((nint)Handle.Value));
            xamlSource.ShouldConstrainPopupsToWorkArea = true;
            xamlSource.GotFocus += XamlSourceGotFocus;
            xamlSource.TakeFocusRequested += XamlSourceTakeFocusRequested;
            ResizeSiteBridge(xamlSource, this.GetClientRectangle().Size);
            xamlSource.Content = content;
            return xamlSource;
        }
        catch
        {
            xamlSource.GotFocus -= XamlSourceGotFocus;
            xamlSource.TakeFocusRequested -= XamlSourceTakeFocusRequested;
            xamlSource.Dispose();
            throw;
        }
    }

    private void DetachXamlSource(out UIElement? content)
    {
        DesktopWindowXamlSource xamlSource = GetXamlSource();
        content = xamlSource.Content;
        _xamlSource = null;
        _reportedFocusRequestId = Guid.Empty;
        xamlSource.GotFocus -= XamlSourceGotFocus;
        xamlSource.TakeFocusRequested -= XamlSourceTakeFocusRequested;
        try
        {
            xamlSource.Content = null;
        }
        finally
        {
            xamlSource.Dispose();
        }
    }

    private DesktopWindowXamlSource GetXamlSource()
    {
        VerifyAccess();
        ObjectDisposedException.ThrowIf(_xamlStateDisposed, this);
        return _xamlSource ?? throw new InvalidOperationException("The WinUI XAML source has not been initialized.");
    }

    private void DisposeXamlState()
    {
        if (_xamlStateDisposed)
        {
            return;
        }

        _xamlStateDisposed = true;
        DesktopWindowXamlSource? xamlSource = _xamlSource;
        XamlHostEnvironment? environment = _environment;
        ShutdownRegistration shutdownRegistration = _shutdownRegistration;
        _xamlSource = null;
        _context = null;
        _environment = null;
        _shutdownRegistration = default;
        _reportedFocusRequestId = Guid.Empty;

        try
        {
            shutdownRegistration.Dispose();
        }
        finally
        {
            try
            {
                if (xamlSource is not null)
                {
                    xamlSource.GotFocus -= XamlSourceGotFocus;
                    xamlSource.TakeFocusRequested -= XamlSourceTakeFocusRequested;
                    try
                    {
                        xamlSource.Content = null;
                    }
                    finally
                    {
                        xamlSource.Dispose();
                    }
                }
            }
            finally
            {
                environment?.Dispose();
            }
        }
    }

    private void NavigateIntoXaml()
    {
        DesktopWindowXamlSource? xamlSource = _xamlSource;
        if (xamlSource is null)
        {
            return;
        }

        bool forward = XamlFocusNavigation.EntryIsForward;
        try
        {
            XamlSourceFocusNavigationRequest request = new(
                forward ? XamlSourceFocusNavigationReason.First : XamlSourceFocusNavigationReason.Last);
            XamlSourceFocusNavigationResult result = xamlSource.NavigateFocus(request);
            if (result.WasFocusMoved)
            {
                ReportXamlGotFocus(request.CorrelationId);
            }
            else
            {
                _ = XamlFocusNavigation.TryMoveFocus(Handle, forward);
            }
        }
        catch (Exception exception)
        {
            ReportNativeCallbackFailure("NavigateFocus", exception);
        }
    }

    private void ApplyApplicationTheme(UIElement? content)
    {
        if (content is not FrameworkElement element)
        {
            _applicationRequestedTheme = null;
            return;
        }

        if (_applicationRequestedTheme is null && element.RequestedTheme != ElementTheme.Default)
        {
            return;
        }

        if (_applicationRequestedTheme is { } previousTheme && element.RequestedTheme != previousTheme)
        {
            _applicationRequestedTheme = null;
            return;
        }

        ElementTheme theme = Application.ColorMode switch
        {
            ApplicationColorMode.Dark => ElementTheme.Dark,
            ApplicationColorMode.Light => ElementTheme.Light,
            _ => ElementTheme.Default
        };
        element.RequestedTheme = theme;
        _applicationRequestedTheme = theme;
    }

    private void XamlSourceGotFocus(
        DesktopWindowXamlSource sender,
        DesktopWindowXamlSourceGotFocusEventArgs eventArgs)
        => ReportXamlGotFocus(eventArgs.Request.CorrelationId);

    private void ReportXamlGotFocus(Guid correlationId)
    {
        if (correlationId != Guid.Empty && correlationId == _reportedFocusRequestId)
        {
            return;
        }

        _reportedFocusRequestId = correlationId;
        try
        {
            XamlGotFocus?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            ReportNativeCallbackFailure("XamlGotFocus", exception);
        }
    }

    private void XamlSourceTakeFocusRequested(
        DesktopWindowXamlSource sender,
        DesktopWindowXamlSourceTakeFocusRequestedEventArgs eventArgs)
    {
        bool? forward = eventArgs.Request.Reason switch
        {
            XamlSourceFocusNavigationReason.First => true,
            XamlSourceFocusNavigationReason.Last => false,
            _ => null
        };

        if (!forward.HasValue)
        {
            return;
        }

        try
        {
            _ = XamlFocusNavigation.TryMoveFocus(Handle, forward.Value);
        }
        catch (Exception exception)
        {
            ReportNativeCallbackFailure("TakeFocusRequested", exception);
        }
    }
}