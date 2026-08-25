using global::Windows.ApplicationModel.DataTransfer;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WinUIHostingDragRepro;

internal static class DirectEditorDragContent
{
    internal static FrameworkElement Create(string hostName)
    {
        TextBlock status = new()
        {
            Text = $"{hostName}: no drag observed.",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(status, "DirectStatus");

        TextBlock textBlock = new()
        {
            CanDrag = true,
            Text = "Control case: drag this TextBlock."
        };
        AutomationProperties.SetAutomationId(textBlock, "DirectTextBlockSource");
        ConfigureSource(textBlock, () => textBlock.Text, "TextBlock", hostName, status);

        TextBox textBox = new()
        {
            AcceptsReturn = true,
            CanDrag = true,
            Header = "Direct TextBox source",
            Text = "Select text, then attempt to drag from the selection.",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(textBox, "DirectTextBoxSource");
        ConfigureSource(textBox, () => textBox.SelectedText, "TextBox", hostName, status);

        RichEditBox richEditBox = new()
        {
            AcceptsReturn = true,
            CanDrag = true,
            Header = "Direct RichEditBox source",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(richEditBox, "DirectRichEditSource");
        richEditBox.Document.SetText(
            TextSetOptions.None,
            "Select text, then attempt to drag from the RichEditBox selection.");
        ConfigureSource(
            richEditBox,
            () => richEditBox.Document.Selection.Text,
            "RichEditBox",
            hostName,
            status);

        TextBox target = new()
        {
            AcceptsReturn = true,
            AllowDrop = true,
            Background = new SolidColorBrush(Colors.White),
            Header = "Copy target",
            MinHeight = 80,
            PlaceholderText = "Drop a successful source here"
        };
        AutomationProperties.SetAutomationId(target, "DirectDropTarget");
        target.DragOver += (_, eventArgs) =>
        {
            eventArgs.AcceptedOperation = eventArgs.DataView.Contains(StandardDataFormats.Text)
                ? DataPackageOperation.Copy
                : DataPackageOperation.None;
            status.Text = eventArgs.AcceptedOperation == DataPackageOperation.Copy
                ? $"{hostName}: target accepted Copy."
                : $"{hostName}: target rejected the offered data.";
        };
        target.Drop += async (_, eventArgs) =>
        {
            DragOperationDeferral deferral = eventArgs.GetDeferral();
            try
            {
                if (!eventArgs.DataView.Contains(StandardDataFormats.Text))
                {
                    eventArgs.AcceptedOperation = DataPackageOperation.None;
                    return;
                }

                target.SelectedText = await eventArgs.DataView.GetTextAsync();
                eventArgs.AcceptedOperation = DataPackageOperation.Copy;
            }
            catch (Exception exception)
            {
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                status.Text = $"{hostName}: drop failed with {exception.GetType().Name}.";
            }
            finally
            {
                deferral.Complete();
            }
        };

        StackPanel panel = new()
        {
            Padding = new Thickness(24),
            RequestedTheme = ElementTheme.Light,
            Spacing = 12
        };
        panel.Children.Add(new TextBlock
        {
            FontSize = 24,
            Text = $"Direct editor drag diagnostic: {hostName}"
        });
        panel.Children.Add(new TextBlock
        {
            Text = "No custom pointer handling is installed. The TextBlock is the control case.",
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(textBlock);
        panel.Children.Add(textBox);
        panel.Children.Add(richEditBox);
        panel.Children.Add(target);
        panel.Children.Add(status);
        return new ScrollViewer { Content = panel };
    }

    private static void ConfigureSource(
        UIElement source,
        Func<string> getText,
        string sourceName,
        string hostName,
        TextBlock status)
    {
        source.DragStarting += (_, eventArgs) =>
        {
            string text = getText();
            status.Text = $"{hostName}: {sourceName} raised DragStarting with {text.Length} characters.";
            if (text.Length == 0)
            {
                eventArgs.Cancel = true;
                return;
            }

            eventArgs.Data.SetText(text);
            eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
            eventArgs.AllowedOperations = DataPackageOperation.Copy;
            eventArgs.DragUI.SetContentFromDataPackage();
        };
        source.DropCompleted += (_, eventArgs) =>
            status.Text = $"{hostName}: {sourceName} completed with {eventArgs.DropResult}.";
    }
}