// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;
using ThirtyTwoLayout = Windows.Layout;

namespace DpiTesting;

/// <summary>Displays native and XAML DPI metrics while moving between monitors.</summary>
internal sealed class DpiTestWindow : MainWindow
{
    private static readonly Size s_referenceLogicalSize = new(240, 120);

    private readonly TextLabelControl _title;
    private readonly ButtonControl _refreshButton;
    private readonly TextLabelControl _nativeMetrics;
    private readonly TextLabelControl _nativeReference;
    private readonly DpiObservingHost _host;
    private readonly DpiTestContent _content;
    private readonly TextLabelControl _instructions;
    private readonly Window[] _ownedControls;
    private int _transitionCount;
    private string _lastTransition = "none";

    internal DpiTestWindow()
        : base(
            bounds: new Rectangle(40, 30, 1100, 760),
            title: "thirtytwo WinUI Mixed-DPI Test")
    {
        List<Window> ownedControls = [];
        DpiTestContent? content = null;

        try
        {
            _title = Track(new TextLabelControl(
                text: "WinUI mixed-monitor DPI test",
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _title.SetFont("Segoe UI", 20);

            _refreshButton = Track(new ButtonControl(
                text: "Refresh metrics",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);

            _nativeMetrics = Track(new TextLabelControl(
                textFormat: DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.WordBreak | DrawTextFormat.NoPrefix,
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _nativeMetrics.SetFont("Consolas", 10);

            _nativeReference = Track(new TextLabelControl(
                textFormat: DrawTextFormat.Center | DrawTextFormat.VerticallyCenter | DrawTextFormat.SingleLine,
                text: "240 x 120 logical units",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.Border,
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _nativeReference.SetFont("Segoe UI", 12);

            _host = Track(new DpiObservingHost(default, this, () => content = new DpiTestContent()), ownedControls);
            _content = content ?? throw new InvalidOperationException("The DPI test XAML content factory did not run.");

            _instructions = Track(new TextLabelControl(
                textFormat: DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.WordBreak | DrawTextFormat.NoPrefix,
                text: "Move the window fully onto each monitor, resize and maximize it, then open the XAML popup. Native DPI and XAML scale should agree; both 240 x 120 rulers should remain the same logical size without clipping or blank frames.",
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _instructions.SetFont("Segoe UI", 10);

            _ownedControls = [.. ownedControls];
            _refreshButton.Click += RefreshButtonClick;
            _content.MetricsChanged += ContentMetricsChanged;
            _host.DpiTransition += HostDpiTransition;

            this.AddLayoutHandler(CreateWindowLayout());
            MessageHandler += WindowMessageHandler;
            UpdateMetrics();
        }
        catch
        {
            content?.Dispose();
            for (int index = ownedControls.Count - 1; index >= 0; index--)
            {
                ownedControls[index].Dispose();
            }

            base.Dispose(disposing: true);
            throw;
        }
    }

    private static TControl Track<TControl>(TControl control, List<Window> ownedControls)
        where TControl : Window
    {
        ownedControls.Add(control);
        return control;
    }

    private ILayoutHandler CreateWindowLayout()
    {
        ILayoutHandler titleLayout = ThirtyTwoLayout.Vertical(
            (.8f, ThirtyTwoLayout.Margin((16, 8, 8, 4), ThirtyTwoLayout.Fill(_title))),
            (.2f, ThirtyTwoLayout.Margin((8, 8, 16, 4), ThirtyTwoLayout.Fill(_refreshButton))));

        ILayoutHandler comparisonLayout = ThirtyTwoLayout.Vertical(
            (.35f, ThirtyTwoLayout.Margin(
                (16, 8, 8, 8),
                ThirtyTwoLayout.FixedSize(s_referenceLogicalSize, _nativeReference))),
            (.65f, ThirtyTwoLayout.Margin((8, 8, 16, 8), ThirtyTwoLayout.Fill(_host))));

        return ThirtyTwoLayout.Horizontal(
            (.09f, titleLayout),
            (.22f, ThirtyTwoLayout.Margin((16, 4, 16, 4), ThirtyTwoLayout.Fill(_nativeMetrics))),
            (.57f, comparisonLayout),
            (.12f, ThirtyTwoLayout.Margin((16, 4, 16, 12), ThirtyTwoLayout.Fill(_instructions))));
    }

    protected override void OnDpiChanged(uint oldDpi, uint newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        RecordTransition("top-level", oldDpi, newDpi);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            MessageHandler -= WindowMessageHandler;
            _refreshButton.Click -= RefreshButtonClick;
            _content.MetricsChanged -= ContentMetricsChanged;
            _host.DpiTransition -= HostDpiTransition;
            _content.Dispose();
            for (int index = _ownedControls.Length - 1; index >= 0; index--)
            {
                _ownedControls[index].Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private LRESULT? WindowMessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        if (message == MessageType.WindowPositionChanged)
        {
            UpdateMetrics();
        }

        return null;
    }

    private void RefreshButtonClick(object? sender, EventArgs eventArgs) => UpdateMetrics();

    private void ContentMetricsChanged(object? sender, EventArgs eventArgs) => UpdateMetrics();

    private void HostDpiTransition(uint oldDpi, uint newDpi) => RecordTransition("host child", oldDpi, newDpi);

    private void RecordTransition(string source, uint oldDpi, uint newDpi)
    {
        _transitionCount++;
        _lastTransition = $"{source}: {oldDpi} -> {newDpi}";
        UpdateMetrics();
    }

    private void UpdateMetrics()
    {
        if (Handle.IsNull || _host.Handle.IsNull)
        {
            return;
        }

        uint dpi = this.GetDpi();
        float scale = this.GetScale();
        Rectangle windowBounds = this.GetWindowRectangle();
        Size clientSize = this.GetClientRectangle().Size;
        Size hostSize = _host.GetClientRectangle().Size;
        Size nativeReferenceSize = _nativeReference.GetWindowRectangle().Size;
        int expectedNativeWidth = (int)MathF.Round(s_referenceLogicalSize.Width * scale);
        int expectedNativeHeight = (int)MathF.Round(s_referenceLogicalSize.Height * scale);
        double xamlScale = _content.XamlRootScale;
        int expectedXamlWidth = (int)Math.Round(_content.LogicalWidth * xamlScale);
        int expectedXamlHeight = (int)Math.Round(_content.LogicalHeight * xamlScale);
        bool nativeMatches = Math.Abs(nativeReferenceSize.Width - expectedNativeWidth) <= 1
            && Math.Abs(nativeReferenceSize.Height - expectedNativeHeight) <= 1;
        bool xamlMatches = xamlScale > 0
            && Math.Abs(hostSize.Width - expectedXamlWidth) <= 1
            && Math.Abs(hostSize.Height - expectedXamlHeight) <= 1;

        _nativeMetrics.Text = $"Native window DPI: {dpi} ({scale * 100:F0}%)\r\nWindow bounds (virtual-screen px): X={windowBounds.X}, Y={windowBounds.Y}, W={windowBounds.Width}, H={windowBounds.Height}; client={clientSize.Width} x {clientSize.Height} px\r\nHost client: {hostSize.Width} x {hostSize.Height} px; XAML root: {xamlScale:F3} ({xamlScale * 100:F0}%); host/XAML pixels: {(xamlMatches ? "MATCH" : "CHECK")}\r\nNative ruler: {nativeReferenceSize.Width} x {nativeReferenceSize.Height} px; expected {expectedNativeWidth} x {expectedNativeHeight}: {(nativeMatches ? "MATCH" : "CHECK")}\r\nDPI transitions: {_transitionCount}; last: {_lastTransition}";
    }

}
