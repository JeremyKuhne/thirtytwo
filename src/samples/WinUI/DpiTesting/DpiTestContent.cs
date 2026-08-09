// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DpiTesting;

/// <summary>Provides XAML-side DPI metrics and a fixed logical-size reference surface.</summary>
internal sealed class DpiTestContent : Grid, IDisposable
{
    internal const double ReferenceWidth = 240;
    internal const double ReferenceHeight = 120;

    private readonly TextBlock _metrics;
    private XamlRoot? _subscribedRoot;
    private bool _disposed;

    internal DpiTestContent()
    {
        Padding = new Thickness(16);
        RowSpacing = 12;
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition());

        TextBlock title = new()
        {
            Text = "WinUI island",
            FontSize = 22,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };
        Children.Add(title);

        _metrics = new TextBlock
        {
            Text = "Waiting for XamlRoot...",
            TextWrapping = TextWrapping.Wrap
        };
        SetRow(_metrics, 1);
        Children.Add(_metrics);

        Border reference = new()
        {
            Width = ReferenceWidth,
            Height = ReferenceHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            BorderBrush = new SolidColorBrush(Colors.DeepSkyBlue),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = "240 x 120 XAML DIPs",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        SetRow(reference, 2);
        Children.Add(reference);

        ComboBox popup = new()
        {
            Header = "Popup alignment check",
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = ReferenceWidth,
            PlaceholderText = "Open while on each monitor"
        };
        popup.Items.Add("First popup item");
        popup.Items.Add("Second popup item");
        popup.Items.Add("Third popup item");
        SetRow(popup, 3);
        Children.Add(popup);

        TextBlock guidance = new()
        {
            Text = "Native HWND bounds are physical pixels. XAML dimensions are view pixels and should convert using XamlRoot.RasterizationScale.",
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        SetRow(guidance, 4);
        Children.Add(guidance);

        Loaded += ContentLoaded;
        Unloaded += ContentUnloaded;
        SizeChanged += ContentSizeChanged;
    }

    internal event EventHandler? MetricsChanged;

    internal double XamlRootScale => XamlRoot?.RasterizationScale ?? 0;

    internal double LogicalWidth => ActualWidth;

    internal double LogicalHeight => ActualHeight;

    internal string MetricsText => _metrics.Text;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Loaded -= ContentLoaded;
        Unloaded -= ContentUnloaded;
        SizeChanged -= ContentSizeChanged;
        DetachXamlRoot();
    }

    private void ContentLoaded(object sender, RoutedEventArgs eventArgs)
    {
        AttachXamlRoot();
        UpdateMetrics();
    }

    private void ContentUnloaded(object sender, RoutedEventArgs eventArgs) => DetachXamlRoot();

    private void ContentSizeChanged(object sender, SizeChangedEventArgs eventArgs) => UpdateMetrics();

    private void XamlRootChanged(XamlRoot sender, XamlRootChangedEventArgs eventArgs) => UpdateMetrics();

    private void AttachXamlRoot()
    {
        XamlRoot? root = XamlRoot;
        if (ReferenceEquals(root, _subscribedRoot))
        {
            return;
        }

        DetachXamlRoot();
        _subscribedRoot = root;
        if (_subscribedRoot is not null)
        {
            _subscribedRoot.Changed += XamlRootChanged;
        }
    }

    private void DetachXamlRoot()
    {
        if (_subscribedRoot is not null)
        {
            _subscribedRoot.Changed -= XamlRootChanged;
            _subscribedRoot = null;
        }
    }

    private void UpdateMetrics()
    {
        if (_disposed)
        {
            return;
        }

        AttachXamlRoot();
        double scale = XamlRootScale;
        string text = scale == 0
            ? "Waiting for XamlRoot..."
            : $"XamlRoot scale: {scale:F3} ({scale * 100:F0}%)\nContent: {LogicalWidth:F1} x {LogicalHeight:F1} DIPs -> {LogicalWidth * scale:F0} x {LogicalHeight * scale:F0} pixels\nReference: {ReferenceWidth:F0} x {ReferenceHeight:F0} DIPs -> {ReferenceWidth * scale:F0} x {ReferenceHeight * scale:F0} pixels";

        if (_metrics.Text != text)
        {
            _metrics.Text = text;
        }

        MetricsChanged?.Invoke(this, EventArgs.Empty);
    }
}
