using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;

namespace MinimalWinUIHost;

internal sealed class XamlApplication : Microsoft.UI.Xaml.Application, IXamlMetadataProvider, IDisposable
{
    private readonly XamlControlsXamlMetaDataProvider _metadataProvider = new();
    private readonly WindowsXamlManager _xamlManager;
    private bool _disposed;

    internal XamlApplication()
    {
        _xamlManager = WindowsXamlManager.InitializeForCurrentThread();
        Resources.MergedDictionaries.Add(new XamlControlsResources());
    }

    IXamlType? IXamlMetadataProvider.GetXamlType(string fullName)
        => _metadataProvider.GetXamlType(fullName);

    IXamlType? IXamlMetadataProvider.GetXamlType(Type type)
        => _metadataProvider.GetXamlType(type);

    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
        => _metadataProvider.GetXmlnsDefinitions();

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
