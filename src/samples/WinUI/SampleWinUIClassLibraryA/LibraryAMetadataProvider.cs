// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Markup;

namespace SampleWinUIClassLibraryA;

public sealed class LibraryAMetadataProvider : IXamlMetadataProvider
{
    public const string CollisionTypeName = "SampleWinUI.SharedControl";
    private readonly LibraryAXamlType _xamlType = new();

    public IXamlType? GetXamlType(string fullName)
        => fullName == typeof(LibraryAControl).FullName || fullName == CollisionTypeName
            ? _xamlType
            : null;

    public IXamlType? GetXamlType(Type type)
        => type == typeof(LibraryAControl) ? _xamlType : null;

    public XmlnsDefinition[] GetXmlnsDefinitions() => [];
}