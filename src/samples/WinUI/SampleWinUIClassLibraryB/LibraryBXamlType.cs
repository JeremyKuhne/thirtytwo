// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Markup;

namespace SampleWinUIClassLibraryB;

internal sealed class LibraryBXamlType : IXamlType
{
    public IXamlType? BaseType => null;
    public IXamlType? BoxedType => null;
    public IXamlMember? ContentProperty => null;
    public string FullName => typeof(LibraryBControl).FullName!;
    public bool IsArray => false;
    public bool IsBindable => false;
    public bool IsCollection => false;
    public bool IsConstructible => true;
    public bool IsDictionary => false;
    public bool IsMarkupExtension => false;
    public IXamlType? ItemType => null;
    public IXamlType? KeyType => null;
    public Type UnderlyingType => typeof(LibraryBControl);

    public object ActivateInstance() => new LibraryBControl();
    public void AddToMap(object instance, object key, object value) => throw new NotSupportedException();
    public void AddToVector(object instance, object value) => throw new NotSupportedException();
    public object CreateFromString(string value) => throw new NotSupportedException();
    public IXamlMember? GetMember(string name) => null;
    public void RunInitializer() { }
}