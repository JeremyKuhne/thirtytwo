// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Markup;

namespace Windows.WinUI;

/// <summary>
///  Provides metadata and resource composition to the WinUI host environment.
/// </summary>
public interface IXamlHostApplication : IXamlMetadataProvider
{
    /// <summary>
    ///  Gets the metadata providers exposed by the application.
    /// </summary>
    XamlMetadataProviderRegistry MetadataProviders { get; }

    /// <summary>
    ///  Gets the resource dictionaries exposed by the application.
    /// </summary>
    XamlResourceDictionaryRegistry ResourceDictionaries { get; }
}