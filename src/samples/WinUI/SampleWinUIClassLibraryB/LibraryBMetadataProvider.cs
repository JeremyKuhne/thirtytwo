// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Markup;

namespace SampleWinUIClassLibraryB;

public sealed class LibraryBMetadataProvider : IXamlMetadataProvider
{
    public const string CollisionTypeName = "SampleWinUI.SharedControl";
    private readonly LibraryBXamlType _xamlType = new();

    public IXamlType? GetXamlType(string fullName)
        => fullName == typeof(LibraryBControl).FullName || fullName == CollisionTypeName
            ? _xamlType
            : null;

    public IXamlType? GetXamlType(Type type)
        => type == typeof(LibraryBControl) ? _xamlType : null;

    public XmlnsDefinition[] GetXmlnsDefinitions() => [];
}