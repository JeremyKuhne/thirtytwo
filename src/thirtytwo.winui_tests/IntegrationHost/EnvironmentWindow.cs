// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using SampleWinUIClassLibraryA;
using SampleWinUIClassLibraryB;
using Touki.TestSupport;
using Windows;
using Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.WinUI;
using ResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace IntegrationHost;

internal sealed class EnvironmentWindow : Window
{
    private readonly ScenarioReporter _reporter;
    private XamlHostEnvironment? _environment;
    private XamlHostEnvironment? _secondEnvironment;
    private XamlHostControl? _xamlHost;
    private WinUIColorPicker? _colorPickerHost;
    private XamlHostControl? _popupHost;
    private Window? _shutdownParent;
    private XamlHostControl? _shutdownHost;
    private AirspaceScenario? _airspaceScenario;
    private ScrollingScenario? _scrollingScenario;
    private AccessibilityScenario? _accessibilityScenario;
    private FocusScenario? _focusScenario;
    private InputScenario? _inputScenario;

    internal EnvironmentWindow(EnvironmentScenario scenario, ScenarioReporter reporter)
        : base(DefaultBounds, text: "ThirtyTwo WinUI Environment Host")
    {
        _reporter = reporter;
        Execute(scenario);
        reporter.Write("ready", Handle);

        if (!Dispatcher.TryPost(() =>
            {
                ExecuteAfterShow(scenario);
                if (scenario is not (
                    EnvironmentScenario.HostAirspace
                    or EnvironmentScenario.HostScrolling
                    or EnvironmentScenario.HostAccessibility
                    or EnvironmentScenario.FocusTraversal
                    or EnvironmentScenario.InputSemantics))
                {
                    DestroyWindow();
                }
            }))
        {
            throw new InvalidOperationException("Failed to schedule environment host shutdown.");
        }
    }

    private void Execute(EnvironmentScenario scenario)
    {
        switch (scenario)
        {
            case EnvironmentScenario.Owned:
            case EnvironmentScenario.Borrowed:
            case EnvironmentScenario.CompatibleApplication:
                AcquireAndReport();
                break;
            case EnvironmentScenario.Composition:
                AcquireAndReport();
                VerifyComposition();
                break;
            case EnvironmentScenario.MultipleLeases:
                AcquireAndReport();
                _secondEnvironment = XamlHostEnvironment.Acquire();
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 2, "Two leases were not recorded.");
                _reporter.Write("lease-count-two");
                _environment!.Dispose();
                _environment = null;
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "One lease was not retained.");
                _reporter.Write("lease-count-one");
                break;
            case EnvironmentScenario.IncompatibleApplication:
                VerifyIncompatibleApplication();
                break;
            case EnvironmentScenario.MtaRejected:
                VerifyMtaRejected();
                break;
            case EnvironmentScenario.WrongThreadRejected:
                AcquireAndReport();
                VerifyWrongThreadRejected();
                break;
            case EnvironmentScenario.SecondThreadRejected:
                AcquireAndReport();
                VerifySecondThreadRejected();
                break;
            case EnvironmentScenario.FinalRelease:
                AcquireAndReport();
                WeakReference<Microsoft.UI.Xaml.Application> application = new(_environment!.Application);
                XamlHostEnvironment releasedEnvironment = _environment;
                releasedEnvironment.Dispose();
                releasedEnvironment.Dispose();
                _environment = null;
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "The final public lease was not released.");
                _reporter.Write("double-dispose-idempotent");
                _reporter.Write("final-lease-released");
                using (XamlHostEnvironment reacquired = XamlHostEnvironment.Acquire())
                {
                    Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The environment was not reacquired.");
                    Ensure(
                        application.TryGetTarget(out Microsoft.UI.Xaml.Application? target)
                        && ReferenceEquals(target, reacquired.Application),
                        "Reacquisition replaced the process application.");
                    _reporter.Write("environment-reacquired");
                }

                Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "The reacquired lease was not released.");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Ensure(application.TryGetTarget(out _), "The process application was not retained.");
                _reporter.Write("application-retained");
                break;
            case EnvironmentScenario.HostBasic:
                VerifyHostBasic();
                break;
            case EnvironmentScenario.HostColorPicker:
                VerifyHostColorPicker();
                break;
            case EnvironmentScenario.HostStress:
                VerifyHostStress();
                break;
            case EnvironmentScenario.HostMultiple:
                VerifyHostMultiple();
                break;
            case EnvironmentScenario.HostReparent:
                VerifyHostReparent();
                break;
            case EnvironmentScenario.HostReplacement:
                VerifyHostReplacement();
                break;
            case EnvironmentScenario.HostLayout:
            case EnvironmentScenario.HostAirspace:
            case EnvironmentScenario.HostScrolling:
            case EnvironmentScenario.HostAccessibility:
            case EnvironmentScenario.HostPopupClose:
            case EnvironmentScenario.HostShutdownCleanup:
            case EnvironmentScenario.FocusTraversal:
            case EnvironmentScenario.InputSemantics:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }
    }

    internal void VerifyAfterRun(EnvironmentScenario scenario)
    {
        XamlHostControl? xamlHost = scenario switch
        {
            EnvironmentScenario.HostBasic => _xamlHost,
            EnvironmentScenario.HostPopupClose => _popupHost,
            EnvironmentScenario.HostShutdownCleanup => _shutdownHost,
            _ => null
        };

        if (xamlHost is null)
        {
            return;
        }

        Ensure(xamlHost.Handle.IsNull, "Parent destruction left the managed host HWND alive.");
        DesktopWindowXamlSource? xamlSource = xamlHost.TestAccessor.Dynamic._xamlSource;
        Ensure(xamlSource is null, "Parent destruction left the XAML source alive.");
        if (scenario == EnvironmentScenario.HostShutdownCleanup)
        {
            Window shutdownParent = _shutdownParent
                ?? throw new InvalidOperationException("The shutdown parent was not created.");
            Ensure(!shutdownParent.Handle.IsNull, "The shutdown parent was destroyed with the root window.");
            shutdownParent.Dispose();
            _shutdownParent = null;
        }

        string eventName = scenario switch
        {
            EnvironmentScenario.HostPopupClose => "popup-parent-destroyed",
            EnvironmentScenario.HostShutdownCleanup => "host-shutdown-cleaned",
            _ => "host-parent-destroyed"
        };
        _reporter.Write(eventName);
    }

    private void ExecuteAfterShow(EnvironmentScenario scenario)
    {
        switch (scenario)
        {
            case EnvironmentScenario.HostLayout:
                VerifyHostLayout();
                break;
            case EnvironmentScenario.HostAirspace:
                _airspaceScenario = new(this, _reporter);
                _airspaceScenario.Start();
                break;
            case EnvironmentScenario.HostScrolling:
                _scrollingScenario = new(this, _reporter);
                _scrollingScenario.Start();
                break;
            case EnvironmentScenario.HostAccessibility:
                _accessibilityScenario = new(this, _reporter);
                break;
            case EnvironmentScenario.HostPopupClose:
                VerifyHostPopupClose();
                break;
            case EnvironmentScenario.HostShutdownCleanup:
                VerifyHostShutdownCleanup();
                break;
            case EnvironmentScenario.FocusTraversal:
                _focusScenario = new(this, _reporter);
                if (!Dispatcher.TryPost(() => _focusScenario.Start(DestroyWindow)))
                {
                    throw new InvalidOperationException("Failed to schedule focus traversal after XAML content loading.");
                }

                break;
            case EnvironmentScenario.InputSemantics:
                _inputScenario = new(this, _reporter);
                if (!Dispatcher.TryPost(() => _inputScenario.Start(DestroyWindow)))
                {
                    throw new InvalidOperationException("Failed to schedule input validation after XAML content loading.");
                }

                break;
        }
    }

    private void DestroyWindow()
    {
        if (!PInvoke.DestroyWindow(Handle))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    private void VerifyHostBasic()
    {
        bool contentFactoryCalled = false;
        XamlHostContext? hostContext = null;
        _xamlHost = new(new Rectangle(20, 30, 320, 240), this, context =>
        {
            Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The content factory ran before host initialization.");
            hostContext = context;
            contentFactoryCalled = true;
            return new Grid();
        });

        Ensure(contentFactoryCalled, "The host content factory did not run.");
        XamlHostContext createdContext = hostContext
            ?? throw new InvalidOperationException("The content factory did not receive the host context.");
        Ensure(ReferenceEquals(_xamlHost.Context, createdContext), "The content factory received a different host context.");
        Ensure(
            ReferenceEquals(createdContext.Application, Microsoft.UI.Xaml.Application.Current),
            "The host context returned the wrong process application.");
        XamlHostEnvironmentInfo environmentInfo = XamlHostEnvironment.Current
            ?? throw new InvalidOperationException("The host context has no active environment.");
        Ensure(createdContext.OwnsApplication == environmentInfo.OwnsApplication, "The host context returned the wrong application ownership.");
        Ensure(createdContext.OwnsDispatcherQueue == environmentInfo.OwnsDispatcherQueue, "The host context returned the wrong queue ownership.");
        Ensure(
            ReferenceEquals(createdContext.DispatcherQueue, DispatcherQueue.GetForCurrentThread()),
            "The host context returned the wrong dispatcher queue.");
        IXamlHostApplication hostApplication = (IXamlHostApplication)createdContext.Application;
        Ensure(
            ReferenceEquals(createdContext.MetadataProviders, hostApplication.MetadataProviders),
            "The host context returned the wrong metadata registry.");
        Ensure(
            ReferenceEquals(createdContext.ResourceDictionaries, hostApplication.ResourceDictionaries),
            "The host context returned the wrong resource registry.");
        Ensure(_xamlHost.Content is Grid, "The host did not retain its factory content.");
        Grid replacement = new();
        _xamlHost.Content = replacement;
        Ensure(ReferenceEquals(_xamlHost.Content, replacement), "The host did not replace its content.");
        _xamlHost.Content = null;
        Ensure(_xamlHost.Content is null, "The host did not clear its content.");
        _xamlHost.Content = replacement;
        Ensure(ReferenceEquals(Window.FromHandle(_xamlHost), _xamlHost), "The managed host HWND was not registered.");

        Application.ColorMode = ApplicationColorMode.Dark;
        Ensure(replacement.RequestedTheme == Microsoft.UI.Xaml.ElementTheme.Dark, "Dark mode did not reach hosted XAML content.");
        Application.ColorMode = ApplicationColorMode.Light;
        Ensure(replacement.RequestedTheme == Microsoft.UI.Xaml.ElementTheme.Light, "Light mode did not reach hosted XAML content.");
        Application.ColorMode = ApplicationColorMode.System;
        Ensure(replacement.RequestedTheme == Microsoft.UI.Xaml.ElementTheme.Default, "System mode did not restore the default XAML theme.");
        replacement.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
        Application.ColorMode = ApplicationColorMode.Dark;
        Ensure(replacement.RequestedTheme == Microsoft.UI.Xaml.ElementTheme.Light, "An explicit XAML theme override was replaced.");
        Application.ColorMode = ApplicationColorMode.System;

        XamlHostContext? disposedContext = null;
        using (XamlHostControl temporaryHost = new(
            new Rectangle(0, 0, 10, 10),
            this,
            context =>
            {
                disposedContext = context;
                return new Grid();
            }))
        {
            XamlHostContext temporaryContext = disposedContext
                ?? throw new InvalidOperationException("The temporary content factory did not receive the host context.");
            Ensure(temporaryContext.OwnsApplication == _xamlHost.Context.OwnsApplication, "Host contexts disagreed on application ownership.");
        }

        XamlHostContext retainedContext = disposedContext
            ?? throw new InvalidOperationException("The temporary host context was not retained for disposal validation.");

        try
        {
            _ = retainedContext.OwnsApplication;
            throw new InvalidOperationException("A retained host context remained usable after host disposal.");
        }
        catch (ObjectDisposedException)
        {
        }

        Exception? wrongThreadFailure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = _xamlHost.Content;
            }
            catch (Exception exception)
            {
                wrongThreadFailure = exception;
            }
        });
        thread.Start();
        thread.Join();
        Ensure(wrongThreadFailure is InvalidOperationException, "Wrong-thread host access was not rejected.");
        Ensure(ReferenceEquals(_xamlHost.Content, replacement), "Wrong-thread access changed the hosted content.");

        DesktopWindowXamlSource xamlSource = _xamlHost.TestAccessor.Dynamic._xamlSource;
        HWND siteBridge = (HWND)Win32Interop.GetWindowFromWindowId(xamlSource.SiteBridge.WindowId);
        Ensure(!siteBridge.IsNull, "The XAML source did not create a site-bridge HWND.");
        Ensure(Window.FromHandle(siteBridge) is null, "The WinUI-owned site bridge was registered as a managed window.");
        Ensure(PInvoke.GetParent(siteBridge) == _xamlHost.Handle, "The site bridge is not parented to the managed host.");
        _reporter.Write("host-attached", _xamlHost.Handle);
        _reporter.Write("site-bridge-owned-by-winui", siteBridge);
        _reporter.Write("host-content-created");
        _reporter.Write("host-wrong-thread-rejected");
    }

    private void VerifyHostColorPicker()
    {
        _colorPickerHost = new(new Rectangle(20, 30, 400, 300), this);
        WinUIColorChangedEventArgs? observedChange = null;
        _colorPickerHost.ColorChanged += (_, eventArgs) => observedChange = eventArgs;
        _colorPickerHost.IsAlphaEnabled = true;
        _colorPickerHost.IsColorSpectrumVisible = false;
        _colorPickerHost.IsColorPreviewVisible = false;
        _colorPickerHost.IsColorSliderVisible = false;
        _colorPickerHost.IsColorChannelTextInputVisible = false;
        _colorPickerHost.IsAlphaSliderVisible = false;
        _colorPickerHost.IsAlphaTextInputVisible = false;
        _colorPickerHost.IsHexInputVisible = false;
        foreach (WinUIColorSpectrumShape shape in Enum.GetValues<WinUIColorSpectrumShape>())
        {
            _colorPickerHost.ColorSpectrumShape = shape;
            Ensure(_colorPickerHost.ColorSpectrumShape == shape, $"The {shape} spectrum shape did not round-trip.");
        }

        foreach (WinUIColorSpectrumComponents components in Enum.GetValues<WinUIColorSpectrumComponents>())
        {
            _colorPickerHost.ColorSpectrumComponents = components;
            Ensure(_colorPickerHost.ColorSpectrumComponents == components, $"The {components} spectrum mapping did not round-trip.");
        }

        foreach (WinUIColorPickerOrientation orientation in Enum.GetValues<WinUIColorPickerOrientation>())
        {
            _colorPickerHost.Orientation = orientation;
            Ensure(_colorPickerHost.Orientation == orientation, $"The {orientation} orientation did not round-trip.");
        }

        foreach (WinUIElementTheme theme in Enum.GetValues<WinUIElementTheme>())
        {
            _colorPickerHost.RequestedTheme = theme;
            Ensure(_colorPickerHost.RequestedTheme == theme, $"The {theme} requested theme did not round-trip.");
        }

        EnsureThrows<ArgumentOutOfRangeException>(
            () => _colorPickerHost.ColorSpectrumShape = (WinUIColorSpectrumShape)int.MaxValue,
            "The color picker accepted an unknown spectrum shape.");
        EnsureThrows<ArgumentOutOfRangeException>(
            () => _colorPickerHost.ColorSpectrumComponents = (WinUIColorSpectrumComponents)int.MaxValue,
            "The color picker accepted an unknown spectrum mapping.");
        EnsureThrows<ArgumentOutOfRangeException>(
            () => _colorPickerHost.Orientation = (WinUIColorPickerOrientation)int.MaxValue,
            "The color picker accepted an unknown orientation.");
        EnsureThrows<ArgumentOutOfRangeException>(
            () => _colorPickerHost.RequestedTheme = (WinUIElementTheme)int.MaxValue,
            "The color picker accepted an unknown requested theme.");
        Color expected = Color.FromArgb(128, 12, 34, 56);

        _colorPickerHost.Color = expected;

        Ensure(_colorPickerHost.IsAlphaEnabled, "The alpha-enabled setting was not retained.");
        Ensure(!_colorPickerHost.IsColorSpectrumVisible, "The color-spectrum visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsColorPreviewVisible, "The color-preview visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsColorSliderVisible, "The color-slider visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsColorChannelTextInputVisible, "The channel-input visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsAlphaSliderVisible, "The alpha-slider visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsAlphaTextInputVisible, "The alpha-input visibility setting was not retained.");
        Ensure(!_colorPickerHost.IsHexInputVisible, "The hexadecimal-input visibility setting was not retained.");
        Ensure(_colorPickerHost.ColorSpectrumShape == WinUIColorSpectrumShape.Ring, "The spectrum shape was not retained.");
        Ensure(
            _colorPickerHost.ColorSpectrumComponents == WinUIColorSpectrumComponents.ValueSaturation,
            "The spectrum-component mapping was not retained.");
        Ensure(_colorPickerHost.Orientation == WinUIColorPickerOrientation.Horizontal, "The orientation was not retained.");
        Ensure(_colorPickerHost.RequestedTheme == WinUIElementTheme.Dark, "The requested theme was not retained.");
        Ensure(_colorPickerHost.Color == expected, "The selected color did not round-trip through the wrapper.");
        Ensure(observedChange?.NewColor == expected, "The projected color-change event reported the wrong color.");
        try
        {
            _colorPickerHost.Content = new Grid();
            throw new InvalidOperationException("The typed wrapper accepted replacement content.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "WinUIColorPicker content cannot be replaced.")
        {
        }

        Ensure(_colorPickerHost.Color == expected, "Rejected content replacement changed the selected color.");
        _reporter.Write("color-picker-projected", _colorPickerHost.Handle);
    }

    private void VerifyHostStress()
    {
        int initialChildCount = CountChildWindows();
        try
        {
            _ = new XamlHostControl(
                new Rectangle(0, 0, 10, 10),
                this,
                static context => throw new InvalidOperationException("Expected content factory failure."));
            throw new InvalidOperationException("A throwing content factory did not fail host construction.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "Expected content factory failure.")
        {
        }

        try
        {
            _ = new XamlHostControl(
                new Rectangle(0, 0, 10, 10),
                this,
                (Func<XamlHostContext, Microsoft.UI.Xaml.UIElement>)null!);
            throw new InvalidOperationException("A null content factory did not fail host construction.");
        }
        catch (ArgumentNullException exception) when (exception.ParamName == "contentFactory")
        {
        }

        Ensure(CountChildWindows() == initialChildCount, "Failed host construction left native child windows behind.");
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "Failed host construction retained an environment lease.");
        _reporter.Write("host-constructor-failure-cleaned");

        for (int iteration = 0; iteration < 1_000; iteration++)
        {
            using XamlHostControl host = new(new Rectangle(0, 0, 1 + (iteration % 7), 1 + (iteration % 11)), this);
            HWND handle = host.Handle;
            Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "A stress host did not acquire one lease.");

            host.Dispose();

            Ensure(host.Handle.IsNull, "A stress host retained its managed HWND after disposal.");
            Ensure(Window.FromHandle(handle) is null, "A disposed stress host remained in managed HWND lookup.");
            Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "A stress host retained its environment lease.");
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Ensure(CountChildWindows() == initialChildCount, "Host stress left native child windows behind.");
        _reporter.Write("host-stress-completed");
    }

    private void VerifyHostMultiple()
    {
        using XamlHostControl first = new(new Rectangle(0, 0, 80, 60), this, static () => new Grid());
        using XamlHostControl second = new(new Rectangle(80, 0, 80, 60), this, static () => new Grid());
        using XamlHostControl third = new(new Rectangle(160, 0, 80, 60), this, static () => new Grid());
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 3, "Three hosts did not acquire three leases.");

        second.Dispose();
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 2, "Disposing the middle host did not release one lease.");
        Ensure(first.Content is Grid && third.Content is Grid, "Disposing one host invalidated another host.");
        first.Dispose();
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "Disposing the first host did not release one lease.");
        third.Dispose();
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "Disposing all hosts retained a lease.");

        using XamlHostControl fourth = new(new Rectangle(0, 60, 80, 60), this);
        using XamlHostControl fifth = new(new Rectangle(80, 60, 80, 60), this);
        fifth.Dispose();
        fourth.Dispose();
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "Reverse-order disposal retained a lease.");
        _reporter.Write("multiple-host-disposal-completed");
    }

    private unsafe void VerifyHostLayout()
    {
        using XamlHostControl host = new(new Rectangle(0, 0, 1, 1), this, static () => new Grid());
        DesktopWindowXamlSource xamlSource = GetXamlSource(host);
        HWND siteBridge = GetSiteBridge(xamlSource);

        host.MoveWindow(Rectangle.Empty, repaint: false);
        Ensure(host.GetClientRectangle().Size == Size.Empty, "The managed host did not accept zero size.");
        Ensure(siteBridge.GetClientRectangle().Size == Size.Empty, "The site bridge did not accept zero size.");
        _reporter.Write("host-zero-size");

        host.MoveWindow(new Rectangle(10, 10, 120, 90), repaint: false);
        host.ShowWindow(ShowWindowCommand.Hide);
        Ensure(!PInvoke.IsWindowVisible(host.Handle), "The managed host remained visible after hide.");
        Ensure(!PInvoke.IsWindowVisible(siteBridge), "The site bridge remained visible after its host was hidden.");
        host.ShowWindow(ShowWindowCommand.Show);
        Ensure(PInvoke.IsWindowVisible(host.Handle), "The managed host did not become visible.");
        Ensure(PInvoke.IsWindowVisible(siteBridge), "The site bridge did not become visible with its host.");
        _reporter.Write("host-visibility-synchronized");

        Size expectedSize = default;
        for (int iteration = 1; iteration <= 250; iteration++)
        {
            expectedSize = new(1 + ((iteration * 17) % 480), 1 + ((iteration * 29) % 320));
            host.MoveWindow(new Rectangle(iteration % 31, iteration % 23, expectedSize.Width, expectedSize.Height), repaint: false);
        }

        Ensure(siteBridge.GetClientRectangle().Size == expectedSize, "The site bridge did not track the resize storm.");
        Ensure(ReferenceEquals(GetXamlSource(host), xamlSource), "Resizing replaced the XAML source.");
        _reporter.Write("host-resize-storm-completed");

        uint oldDpi = host.GetDpi();
        ushort newDpi = checked((ushort)(oldDpi + 24));
        Rectangle dpiBounds = new(15, 20, 360, 240);
        RECT suggestedBounds = dpiBounds;
        nuint packedDpi = newDpi | ((nuint)newDpi << 16);

        // SendMessage is synchronous, so the stack RECT remains valid until the window procedure returns.
        _ = host.SendMessage(
            MessageType.DpiChanged,
            (WPARAM)packedDpi,
            (LPARAM)(nint)(&suggestedBounds));

        Ensure(host.GetClientRectangle().Size == dpiBounds.Size, "The managed host did not accept DPI-adjusted bounds.");
        Ensure(siteBridge.GetClientRectangle().Size == dpiBounds.Size, "The site bridge did not resynchronize after DPI change.");
        Ensure(ReferenceEquals(GetXamlSource(host), xamlSource), "DPI change replaced the XAML source.");
        _reporter.Write("host-dpi-resynchronized");
    }

    private void VerifyHostReparent()
    {
        using CustomControl firstParent = new(new Rectangle(0, 0, 300, 220), parentWindow: this);
        using CustomControl secondParent = new(new Rectangle(300, 0, 300, 220), parentWindow: this);
        using XamlHostControl host = new(new Rectangle(5, 7, 180, 140), firstParent, static () => new Grid());
        DesktopWindowXamlSource xamlSource = GetXamlSource(host);
        HWND siteBridge = GetSiteBridge(xamlSource);
        using CustomControl destroyedParent = new(new Rectangle(0, 220, 100, 80), parentWindow: this);
        destroyedParent.Dispose();

        try
        {
            host.Reparent(destroyedParent);
            throw new InvalidOperationException("A destroyed reparent target was accepted.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "The new parent window has been destroyed.")
        {
        }

        Ensure(PInvoke.GetParent(host.Handle) == firstParent.Handle, "Rejected reparenting changed the native parent.");
        Ensure(ReferenceEquals(GetXamlSource(host), xamlSource), "Rejected reparenting replaced the XAML source.");
        Ensure(host.Content is Grid, "Rejected reparenting lost the hosted content.");
        _reporter.Write("destroyed-reparent-target-rejected");

        host.Reparent(secondParent);
        host.MoveWindow(new Rectangle(11, 13, 200, 150), repaint: false);

        Ensure(PInvoke.GetParent(host.Handle) == secondParent.Handle, "The host did not move to the new parent.");
        HWND reattachedSiteBridge = GetSiteBridge(GetXamlSource(host));
        Ensure(PInvoke.GetParent(reattachedSiteBridge) == host.Handle, "Reparenting detached the site bridge from its host.");
        Ensure(!ReferenceEquals(GetXamlSource(host), xamlSource), "Reparenting reused the source attached to the old parent.");
        Ensure(reattachedSiteBridge != siteBridge, "Reparenting reused the site bridge attached to the old parent.");
        Ensure(host.Content is Grid, "Reparenting lost the hosted content.");
        _reporter.Write("host-reparented");
    }

    private void VerifyHostReplacement()
    {
        DesktopWindowXamlSource firstSource;
        using (XamlHostControl first = new(new Rectangle(0, 0, 160, 120), this, static () => new Grid()))
        {
            firstSource = GetXamlSource(first);
        }

        using XamlHostControl replacement = new(new Rectangle(0, 0, 160, 120), this, static () => new Grid());
        Ensure(!ReferenceEquals(GetXamlSource(replacement), firstSource), "A replacement host reused a disposed XAML source.");
        Ensure(replacement.Content is Grid, "The replacement host did not create content.");
        _reporter.Write("host-replacement-created");
    }

    private void VerifyHostPopupClose()
    {
        ComboBox? comboBox = null;
        _popupHost = new(new Rectangle(20, 30, 240, 80), this, () => comboBox = new());
        ComboBox createdComboBox = comboBox
            ?? throw new InvalidOperationException("The popup content factory did not run.");
        createdComboBox.Items.Add("First");
        createdComboBox.Items.Add("Second");
        createdComboBox.IsDropDownOpen = true;
        Ensure(createdComboBox.IsDropDownOpen, "The hosted popup did not open.");
        _reporter.Write("host-popup-open", _popupHost.Handle);
    }

    private void VerifyHostShutdownCleanup()
    {
        _shutdownParent = new(new Rectangle(0, 0, 320, 240), text: "Shutdown cleanup parent");
        _shutdownHost = new(new Rectangle(20, 30, 200, 150), _shutdownParent, static () => new Grid());
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The shutdown-cleanup host did not acquire a lease.");
        _reporter.Write("host-left-for-shutdown", _shutdownHost.Handle);
    }

    private int CountChildWindows()
    {
        int count = 0;
        this.EnumerateChildWindows(_ =>
        {
            count++;
            return true;
        });
        return count;
    }

    private static DesktopWindowXamlSource GetXamlSource(XamlHostControl host)
        => host.TestAccessor.Dynamic._xamlSource
            ?? throw new InvalidOperationException("The host has no XAML source.");

    private static HWND GetSiteBridge(DesktopWindowXamlSource xamlSource)
        => (HWND)Win32Interop.GetWindowFromWindowId(xamlSource.SiteBridge.WindowId);

    private void AcquireAndReport()
    {
        _environment = XamlHostEnvironment.Acquire();
        _reporter.Write(_environment.OwnsDispatcherQueue ? "queue-owned" : "queue-borrowed");
        _reporter.Write(_environment.OwnsApplication ? "application-owned" : "application-borrowed");
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The first lease was not recorded.");
        _reporter.Write("environment-acquired");
    }

    private void VerifyComposition()
    {
        XamlMetadataProviderRegistry metadata = _environment!.MetadataProviders;
        XamlResourceDictionaryRegistry resources = _environment.ResourceDictionaries;
        int metadataCollisions = 0;
        int resourceCollisions = 0;
        metadata.CollisionDetected += (_, _) => metadataCollisions++;

        LibraryAMetadataProvider providerA = new();
        LibraryBMetadataProvider providerB = new();
        Ensure(metadata.Register(providerA), "Provider A was not registered.");
        Ensure(!metadata.Register(new LibraryAMetadataProvider()), "Provider A registration was not idempotent.");
        Ensure(metadata.Register(providerB), "Provider B was not registered.");

        IXamlType typeA = metadata.GetXamlType(typeof(LibraryAControl))
            ?? throw new InvalidOperationException("Provider A did not resolve its custom type.");
        IXamlType typeB = metadata.GetXamlType(typeof(LibraryBControl))
            ?? throw new InvalidOperationException("Provider B did not resolve its custom type.");
        Ensure(typeA.ActivateInstance() is LibraryAControl, "Provider A activated the wrong type.");
        Ensure(typeB.ActivateInstance() is LibraryBControl, "Provider B activated the wrong type.");
        IXamlType collision = metadata.GetXamlType(LibraryAMetadataProvider.CollisionTypeName)
            ?? throw new InvalidOperationException("The collision alias was not resolved.");
        Ensure(collision.UnderlyingType == typeof(LibraryAControl), "Metadata lookup did not preserve first-provider precedence.");
        Ensure(metadataCollisions == 1, "The metadata collision was not reported exactly once.");
        _reporter.Write("metadata-composed");
        _reporter.Write("metadata-collision-reported");

        LibraryAResources resourcesA = new();
        LibraryBResources resourcesB = new();
        Ensure(resources.Register(resourcesA), "Resources A were not registered.");
        Ensure(!resources.Register(resourcesA), "Resource registration was not idempotent.");
        VerifyResourceIndexFailureRollsBack(resources);
        EventHandler<XamlResourceCollisionEventArgs> throwingHandler =
            static (_, _) => throw new InvalidOperationException("Expected collision callback failure.");
        resources.CollisionDetected += throwingHandler;
        try
        {
            _ = resources.Register(resourcesB);
            throw new InvalidOperationException("A throwing collision callback did not fail registration.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "Expected collision callback failure.")
        {
            _reporter.Write("resource-registration-rolled-back");
        }
        finally
        {
            resources.CollisionDetected -= throwingHandler;
        }

        resources.CollisionDetected += (_, _) => resourceCollisions++;
        Ensure(resources.Register(resourcesB), "Resources B were not registered.");
        Ensure(resourceCollisions == 1, "The resource collision was not reported exactly once.");
        Ensure(
            Equals(_environment.Application.Resources[LibraryAResources.SharedResourceKey], "LibraryB"),
            "Later resource registration did not win duplicate-key lookup.");
        Ensure(resourcesA.ThemeDictionaries.ContainsKey("Default"), "Resources A lost its theme dictionary.");
        Ensure(resourcesB.ThemeDictionaries.ContainsKey("Default"), "Resources B lost its theme dictionary.");
        _reporter.Write("resources-composed");
        _reporter.Write("resource-collision-reported");
        _reporter.Write("theme-dictionaries-preserved");
    }

    private void VerifyResourceIndexFailureRollsBack(XamlResourceDictionaryRegistry resources)
    {
        const string indexedBeforeFailure = "SampleWinUI.IndexedBeforeFailure";
        const string failureKey = "SampleWinUI.IndexFailure";
        ResourceDictionary failingDictionary = new();
        failingDictionary[indexedBeforeFailure] = "Expected";
        failingDictionary[failureKey] = "Expected";
        DelayedHashCodeFailureComparer comparer = new();
        dynamic accessor = resources.TestAccessor.Dynamic;
        Dictionary<object, ResourceDictionary> resourceOwners = accessor._resourceOwners;
        accessor._resourceOwners = new Dictionary<object, ResourceDictionary>(resourceOwners, comparer);
        comparer.FailAfterSuccessfulCalls(failureKey, 1);
        EventHandler<XamlResourceCollisionEventArgs> collisionObserver = static (_, _) => { };
        resources.CollisionDetected += collisionObserver;
        int dictionaryCount = resources.Count;
        int mergedDictionaryCount = _environment!.Application.Resources.MergedDictionaries.Count;
        bool expectedFailureObserved = false;
        bool ownerIndexChanged = false;

        try
        {
            _ = resources.Register(failingDictionary);
        }
        catch (InvalidOperationException exception) when (exception.Message == "Expected resource key hash failure.")
        {
            expectedFailureObserved = true;
        }
        finally
        {
            resources.CollisionDetected -= collisionObserver;
            comparer.DisableFailure();
            Dictionary<object, ResourceDictionary> currentResourceOwners = accessor._resourceOwners;
            ownerIndexChanged = currentResourceOwners.ContainsKey(indexedBeforeFailure);
            accessor._resourceOwners = resourceOwners;
        }

        Ensure(expectedFailureObserved, "Resource owner indexing did not fail as expected.");
        Ensure(!ownerIndexChanged, "Failed resource owner indexing partially updated the owner index.");
        Ensure(resources.Count == dictionaryCount, "Failed resource owner indexing changed the registry count.");
        Ensure(!resources.Dictionaries.Contains(failingDictionary), "Failed resource owner indexing retained the dictionary.");
        Ensure(
            _environment.Application.Resources.MergedDictionaries.Count == mergedDictionaryCount
                && !_environment.Application.Resources.MergedDictionaries.Contains(failingDictionary),
            "Failed resource owner indexing mutated the application resources.");
        Ensure(resources.Register(failingDictionary), "The dictionary could not be registered after rollback.");
    }

    private void VerifyIncompatibleApplication()
    {
        try
        {
            _ = XamlHostEnvironment.Acquire();
            throw new InvalidOperationException("An incompatible Application.Current was accepted.");
        }
        catch (XamlHostInitializationException exception) when (
            exception.Stage == XamlHostInitializationStage.Application)
        {
            _reporter.Write("incompatible-application-rejected", message: exception.Message);
        }
    }

    private void VerifyMtaRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = XamlHostEnvironment.Acquire();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        thread.Join();
        if (failure is not XamlHostInitializationException
            {
                Stage: XamlHostInitializationStage.ThreadValidation
            } diagnostic)
        {
            throw new InvalidOperationException("MTA acquisition did not fail with thread diagnostics.", failure);
        }

        Ensure(diagnostic.ManagedThreadId == thread.ManagedThreadId, "The MTA diagnostic did not capture its managed thread.");
        Ensure(diagnostic.NativeThreadId != 0, "The MTA diagnostic did not capture its native thread.");
        _reporter.Write("mta-rejected", message: diagnostic.Message);
    }

    private void VerifyWrongThreadRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _environment!.Dispose();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.Start();
        thread.Join();
        if (failure is not InvalidOperationException diagnostic)
        {
            throw new InvalidOperationException("Wrong-thread disposal was not rejected.", failure);
        }

        _reporter.Write("wrong-thread-rejected", message: diagnostic.Message);
    }

    private void VerifySecondThreadRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = XamlHostEnvironment.Acquire();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not XamlHostInitializationException
            {
                Stage: XamlHostInitializationStage.ThreadValidation
            } diagnostic)
        {
            throw new InvalidOperationException("A second XAML thread was not rejected.", failure);
        }

        _reporter.Write("second-thread-rejected", message: diagnostic.Message);
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void EnsureThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _accessibilityScenario?.Dispose();
            _scrollingScenario?.Dispose();
            _airspaceScenario?.Dispose();
            _inputScenario?.Dispose();
            _focusScenario?.Dispose();
            _popupHost?.Dispose();
            _colorPickerHost?.Dispose();
            _xamlHost?.Dispose();
            _secondEnvironment?.Dispose();
            _environment?.Dispose();
        }

        base.Dispose(disposing);
    }
}