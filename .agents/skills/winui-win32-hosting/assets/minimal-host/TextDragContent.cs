using global::Windows.ApplicationModel.DataTransfer;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MinimalWinUIHost;

internal static class TextDragContent
{
    private const int MaximumDroppedTextLength = 1024 * 1024;

    internal static FrameworkElement Create()
    {
        TextBlock status = new()
        {
            Text = "Select text, then drag its adjacent handle.",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(status, "CanonicalStatus");

        TextBox textBox = new()
        {
            AcceptsReturn = true,
            Header = "TextBox source",
            Text = "Select any part of this plain text.",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(textBox, "CanonicalTextBoxSource");

        RichEditBox richEditBox = new()
        {
            AcceptsReturn = true,
            Header = "RichEditBox source",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(richEditBox, "CanonicalRichEditSource");
        richEditBox.Document.SetText(TextSetOptions.None, "Select any part of this rich text.");

        TextBox target = new()
        {
            AcceptsReturn = true,
            AllowDrop = true,
            Background = new SolidColorBrush(Colors.White),
            Header = "Copy target",
            MinHeight = 96,
            PlaceholderText = "Drop selected text here",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(target, "CanonicalDropTarget");
        target.DragOver += (_, eventArgs) => TargetDragOver(status, eventArgs);
        target.Drop += async (_, eventArgs) =>
            await DropTextAsync(target, status, eventArgs);

        StackPanel panel = new()
        {
            Padding = new Thickness(24),
            RequestedTheme = ElementTheme.Light,
            Spacing = 12
        };
        panel.Children.Add(new TextBlock
        {
            FontSize = 24,
            Text = "WinUI text drag source in a raw HWND host"
        });
        panel.Children.Add(new TextBlock
        {
            Text = "The editors own selection. Separate handles own the drag gesture.",
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(CreateSource(
            textBox,
            () => textBox.SelectedText,
            "Drag TextBox selection",
            "CanonicalTextBoxHandle",
            status));
        panel.Children.Add(CreateSource(
            richEditBox,
            () => richEditBox.Document.Selection.Text,
            "Drag RichEditBox selection",
            "CanonicalRichEditHandle",
            status));
        panel.Children.Add(target);
        panel.Children.Add(status);

        return new ScrollViewer { Content = panel };
    }

    private static FrameworkElement CreateSource(
        Control editor,
        Func<string> getSelectedText,
        string accessibleName,
        string automationId,
        TextBlock status)
    {
        Border dragHandle = new()
        {
            Background = new SolidColorBrush(Colors.LightGray),
            CanDrag = true,
            Child = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = accessibleName
            },
            Padding = new Thickness(12, 8, 12, 8)
        };
        AutomationProperties.SetName(dragHandle, accessibleName);
        AutomationProperties.SetAutomationId(dragHandle, automationId);
        dragHandle.DragStarting += (_, eventArgs) =>
        {
            string selectedText = getSelectedText();
            if (selectedText.Length == 0)
            {
                eventArgs.Cancel = true;
                status.Text = "Select text before starting the drag.";
                return;
            }

            eventArgs.Data.SetText(selectedText);
            eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
            eventArgs.AllowedOperations = DataPackageOperation.Copy;
            eventArgs.DragUI.SetContentFromDataPackage();
            status.Text = $"Dragging {selectedText.Length} selected characters.";
        };
        dragHandle.DropCompleted += (_, eventArgs) =>
            status.Text = $"Drag completed with {eventArgs.DropResult}.";

        StackPanel source = new() { Spacing = 6 };
        source.Children.Add(editor);
        source.Children.Add(dragHandle);
        return source;
    }

    private static void TargetDragOver(TextBlock status, DragEventArgs eventArgs)
    {
        eventArgs.AcceptedOperation = eventArgs.DataView.Contains(StandardDataFormats.Text)
            ? DataPackageOperation.Copy
            : DataPackageOperation.None;
        status.Text = eventArgs.AcceptedOperation == DataPackageOperation.Copy
            ? "Target accepted Copy."
            : "Target rejected the offered data.";
    }

    private static async Task DropTextAsync(TextBox target, TextBlock status, DragEventArgs eventArgs)
    {
        DragOperationDeferral deferral = eventArgs.GetDeferral();
        try
        {
            if (!eventArgs.DataView.Contains(StandardDataFormats.Text))
            {
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            string text = await eventArgs.DataView.GetTextAsync();
            if (text.Length == 0 || text.Length > MaximumDroppedTextLength)
            {
                eventArgs.AcceptedOperation = DataPackageOperation.None;
                status.Text = "The dropped text was empty or exceeded the sample limit.";
                return;
            }

            target.SelectedText = text;
            eventArgs.AcceptedOperation = DataPackageOperation.Copy;
            status.Text = $"Copied {text.Length} characters into the target.";
        }
        catch (Exception exception)
        {
            eventArgs.AcceptedOperation = DataPackageOperation.None;
            status.Text = $"Drop failed: {exception.GetType().Name}.";
        }
        finally
        {
            deferral.Complete();
        }
    }
}