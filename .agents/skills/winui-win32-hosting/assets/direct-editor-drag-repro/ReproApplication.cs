using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;
using WinUIHostingDragRepro;

namespace DirectEditorDragRepro;

internal sealed class ReproApplication : Application, IXamlMetadataProvider
{
    private readonly XamlControlsXamlMetaDataProvider _metadataProvider = new();
    private Window? _window;

    internal ReproApplication()
    {
        _window = new Window
        {
            Content = DirectEditorDragContent.Create("WinUI Window"),
            Title = "Direct editor drag diagnostic"
        };
        _window.Closed += (_, _) => _window = null;
        _window.Activate();
    }

    IXamlType? IXamlMetadataProvider.GetXamlType(string fullName)
        => _metadataProvider.GetXamlType(fullName);

    IXamlType? IXamlMetadataProvider.GetXamlType(Type type)
        => _metadataProvider.GetXamlType(type);

    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
        => _metadataProvider.GetXmlnsDefinitions();
}