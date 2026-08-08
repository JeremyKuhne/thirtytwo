// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;

namespace Windows.WinUI;

/// <summary>
///  Resolves XAML types through registered metadata providers and merges registered resource dictionaries into the
///  application resources.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="XamlHostEnvironment"/> creates this application when the process does not provide an
///   <see cref="IXamlHostApplication"/>.
///  </para>
/// </remarks>
internal sealed class XamlApplication : Microsoft.UI.Xaml.Application, IXamlHostApplication
{
    private XamlMetadataProviderRegistry? _metadataProviders;
    private XamlResourceDictionaryRegistry? _resourceDictionaries;

    public XamlMetadataProviderRegistry MetadataProviders
        => _metadataProviders ?? throw new InvalidOperationException("XAML composition has not been initialized.");

    public XamlResourceDictionaryRegistry ResourceDictionaries
        => _resourceDictionaries ?? throw new InvalidOperationException("XAML composition has not been initialized.");

    /// <summary>
    ///  Initializes the metadata and resource registries with WinUI's built-in controls.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see cref="XamlHostEnvironment"/> calls this on the application's owner thread after
    ///   <see cref="Microsoft.UI.Xaml.Hosting.WindowsXamlManager"/> has initialized XAML for that thread and before the
    ///   application is exposed through the host environment. Calling this again after successful initialization has
    ///   no effect.
    ///  </para>
    /// </remarks>
    internal void InitializeComposition()
    {
        if (_metadataProviders is not null)
        {
            return;
        }

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