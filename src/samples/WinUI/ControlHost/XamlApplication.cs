// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;

namespace ControlHost;

internal sealed class XamlApplication : Microsoft.UI.Xaml.Application, IXamlMetadataProvider, IDisposable
{
    private readonly List<IXamlMetadataProvider> _providers = [new XamlControlsXamlMetaDataProvider()];
    private readonly WindowsXamlManager _xamlManager;
    private bool _disposed;

    public XamlApplication()
    {
        _xamlManager = WindowsXamlManager.InitializeForCurrentThread();
        Resources.MergedDictionaries.Add(new XamlControlsResources());
    }

    public void AddProvider(IXamlMetadataProvider provider)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(provider);
        _providers.Add(provider);
    }

    IXamlType? IXamlMetadataProvider.GetXamlType(string fullName)
    {
        foreach (IXamlMetadataProvider provider in _providers)
        {
            if (provider.GetXamlType(fullName) is IXamlType xamlType)
            {
                return xamlType;
            }
        }

        return null;
    }

    IXamlType? IXamlMetadataProvider.GetXamlType(Type type)
    {
        foreach (IXamlMetadataProvider provider in _providers)
        {
            if (provider.GetXamlType(type) is IXamlType xamlType)
            {
                return xamlType;
            }
        }

        return null;
    }

    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
    {
        List<XmlnsDefinition> definitions = [];
        foreach (IXamlMetadataProvider provider in _providers)
        {
            definitions.AddRange(provider.GetXmlnsDefinitions());
        }

        return [.. definitions];
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _xamlManager.Dispose();
    }
}