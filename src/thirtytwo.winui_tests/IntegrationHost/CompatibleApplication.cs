// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;
using Windows.WinUI;

namespace IntegrationHost;

internal sealed class CompatibleApplication : Microsoft.UI.Xaml.Application, IXamlHostApplication
{
    private XamlMetadataProviderRegistry? _metadataProviders;
    private XamlResourceDictionaryRegistry? _resourceDictionaries;

    public XamlMetadataProviderRegistry MetadataProviders
        => _metadataProviders ?? throw new InvalidOperationException("XAML composition has not been initialized.");

    public XamlResourceDictionaryRegistry ResourceDictionaries
        => _resourceDictionaries ?? throw new InvalidOperationException("XAML composition has not been initialized.");

    internal void InitializeComposition()
    {
        _metadataProviders = new();
        _resourceDictionaries = new(Resources);
        MetadataProviders.Register(new XamlControlsXamlMetaDataProvider());
        ResourceDictionaries.Register(new XamlControlsResources());
    }

    IXamlType? IXamlMetadataProvider.GetXamlType(string fullName)
        => MetadataProviders.GetXamlType(fullName);

    IXamlType? IXamlMetadataProvider.GetXamlType(Type type)
        => MetadataProviders.GetXamlType(type);

    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
        => MetadataProviders.GetXmlnsDefinitions();
}