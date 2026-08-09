// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.WinUI;

namespace IntegrationHost;

internal sealed class AccessibilityScenario : IDisposable
{
    private readonly ScenarioReporter _reporter;
    private readonly XamlHostControl _host;
    private readonly AccessibilityContent _content;
    private bool _disposed;

    internal AccessibilityScenario(Window parent, ScenarioReporter reporter)
    {
        _reporter = reporter;
        AccessibilityContent? content = null;
        XamlHostControl? host = null;
        AccessibilityContent createdContent;
        try
        {
            host = new(
                new Rectangle(20, 20, 760, 620),
                parent,
                () => content = new AccessibilityContent(reporter));
            createdContent = content ?? throw new InvalidOperationException("The accessibility content was not created.");
        }
        catch
        {
            host?.Dispose();
            throw;
        }

        _host = host;
        _content = createdContent;
        reporter.Write("host-accessibility-created", parent.Handle);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _host.Dispose();
        GC.KeepAlive(_content);
        _reporter.Write("accessibility-disposed");
    }
}