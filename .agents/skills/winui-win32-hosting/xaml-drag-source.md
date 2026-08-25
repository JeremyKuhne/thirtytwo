# XAML text drag sources

Use this guide to start a standard text data transfer from ordinary WinUI
content hosted in a `DesktopWindowXamlSource`. Let XAML own the gesture and the
Windows Runtime drag broker. Do not rebuild pointer capture, drag thresholds, or
terminal event handling in the host.

## Applies to

- Windows App SDK 1.4 or later; source and sample checked against 2.3.1.
- .NET 10 on an STA thread.
- Framework-dependent, unpackaged `DesktopWindowXamlSource` hosts.
- Windows 10 version 1809 or later.
- Release builds for x64 and ARM64; automated mouse behavior measured on x64.

The bundled sample is build-tested for both architectures. The automated x64
mouse matrix was measured with Windows App SDK 2.3.1, .NET 10.0.9, Windows build
26200, and a non-elevated process. Touch, pen, ARM64 execution, and external
targets remain separate validation gates.

## Evidence boundary

### Documented contract

For an ordinary `UIElement`, the supported automatic source path is:

1. Set `CanDrag` to `true` on the element that owns the gesture.
2. Populate `DragStartingEventArgs.Data` in `DragStarting`.
3. Set `AllowedOperations` to every operation the source permits.
4. Set `DataPackage.RequestedOperation` to the preferred default.
5. Observe the result in `DropCompleted` when source mutation is required.

`DragUI.SetContentFromDataPackage()` requests a system-provided visual based on
the offered data formats. Without an override, the default visual may represent
the source element itself.

### Source-observed behavior

At the WinUI source commit pinned in [sources.md](sources.md), setting `CanDrag`
creates an `AutomaticDragHelper`. It captures the pointer, applies the system
mouse drag rectangle with an internal multiplier, handles mouse, pen, touch,
release, and capture loss, and calls `StartDragAsync` after recognizing the
gesture.

`TextBoxBase` separately sets `TXTBIT_DISABLEDRAG` for its embedded RichEdit
engine and handles its own routed pointer input. These facts do not establish
that setting `CanDrag` directly on an editable TextBox or RichEditBox produces a
reliable selected-text gesture.

### Measured editable-control behavior

No Microsoft sample located for this guide puts `CanDrag` directly on an
editable TextBox or RichEditBox to drag the current selection. The official XAML
sample instead puts `CanDrag` on a containing element and reads text from an
editor in `DragStarting`.

The bundled diagnostic measured the same content in an ordinary WinUI Window and
a raw `DesktopWindowXamlSource` host. In both hosts:

- the TextBlock control case raised `DragStarting`, completed with Copy, and
  transferred all 34 characters;
- TextBox did not raise `DragStarting`; the pointer gesture changed its selected
  range and left the target empty;
- RichEditBox did not raise `DragStarting`; it followed the same selection
  behavior and left the target empty.

This establishes that the failure is text-control behavior on the measured
baseline, not an island-specific broker failure. This recipe therefore does not
rely on direct editable-surface initiation for Windows App SDK 2.3.1 with x64
mouse input. Re-run the diagnostic when changing the package version or input
device rather than treating this result as a platform-wide guarantee.

## Recommended ownership

Use a separate drag handle for editable text:

| Responsibility | Owner |
| --- | --- |
| Text editing and selection | TextBox or RichEditBox |
| Press, threshold, capture, and drag start | Handle with `CanDrag=true` |
| Transfer payload and allowed effect | Handle's `DragStarting` callback |
| Source completion | Handle's `DropCompleted` callback |
| Acceptance and insertion | Routed XAML target |
| Island-to-system transport | WinUI and the Windows Runtime drag broker |

This shape avoids competing with the editor's click, selection, context-menu,
touch, pen, and capture behavior. In the measured x64 mouse matrix, both explicit
handles completed with Copy, preserved source text and selection, and inserted
the exact selected text into the target. An empty selection canceled before
target mutation.

## Minimal Copy source

Start with Copy. Move adds a source mutation transaction and is not needed to
prove source initiation.

```csharp
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;

private static void ConfigureTextDragHandle(
    UIElement handle,
    Func<string> getSelectedText)
{
    handle.CanDrag = true;
    handle.DragStarting += (_, eventArgs) =>
    {
        string selectedText = getSelectedText();
        if (selectedText.Length == 0)
        {
            eventArgs.Cancel = true;
            return;
        }

        eventArgs.Data.SetText(selectedText);
        eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
        eventArgs.AllowedOperations = DataPackageOperation.Copy;
        eventArgs.DragUI.SetContentFromDataPackage();
    };
}
```

  This Copy-only helper needs no source mutation in `DropCompleted`. Add that
  handler only when implementing the separately reviewed Move transaction below.

Read a TextBox selection through its public property:

```csharp
ConfigureTextDragHandle(textBoxHandle, () => textBox.SelectedText);
```

Read a RichEditBox selection through its text document:

```csharp
ConfigureTextDragHandle(
    richEditBoxHandle,
    () => richEditBox.Document.Selection.Text);
```

The `ITextRange.Text` result is plain Unicode text. Paragraph separators can
reflect the document's original representation. If exact rich formatting is a
requirement, define and validate the additional data formats separately.

## Minimal Copy target

The target needs a non-null background when its otherwise empty area should
participate in hit testing.

```csharp
target.AllowDrop = true;
target.DragOver += (_, eventArgs) =>
{
    eventArgs.AcceptedOperation =
        eventArgs.DataView.Contains(StandardDataFormats.Text)
            ? DataPackageOperation.Copy
            : DataPackageOperation.None;
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

        string text = await eventArgs.DataView.GetTextAsync();
        if (text.Length == 0 || text.Length > MaximumDroppedTextLength)
        {
            eventArgs.AcceptedOperation = DataPackageOperation.None;
            return;
        }

        target.SelectedText = text;
        eventArgs.AcceptedOperation = DataPackageOperation.Copy;
    }
    catch
    {
        eventArgs.AcceptedOperation = DataPackageOperation.None;
    }
    finally
    {
        deferral.Complete();
    }
};
```

Treat incoming data as untrusted even when the expected source is in-process.
Allowlist formats, contain callback exceptions, and cap retained payloads. Text
retrieval itself materializes a string before its length can be checked, so a
target with stronger denial-of-service requirements needs a different bounded
format or transport contract.

## Why not call StartDragAsync from PointerMoved

`StartDragAsync` is the lower-level escape hatch when an application genuinely
owns a custom gesture. Calling it from every `PointerMoved` callback duplicates
the automatic helper's state machine and can start overlapping operations.

Do not combine `CanDrag` with a second manual pointer detector. Choose one gesture
owner. Prefer `CanDrag` for an ordinary XAML source.

## Move is a separate transaction

Only offer Move after Copy is reliable and the editing contract is explicit.
The source must retain enough state to validate the original range when
`DropCompleted` reports Move. A robust editable-text Move must account for:

- source edits or reentrancy while the drag is active;
- cancellation and a target that reports no operation;
- same-editor insertion before or after the original range;
- rejection of insertion inside the source range;
- source unload, island replacement, and host teardown;
- target commit succeeding before source deletion.

If source validation fails after the target inserts a copy, keep the inserted
copy and do not delete an unverified source range. See
[mixed-ole-and-xaml-drag-drop.md](mixed-ole-and-xaml-drag-drop.md) for the full
transaction and cross-framework ownership model.

## Lifecycle

Event subscriptions follow the source element. If application code retains the
element for the host lifetime, clearing `DesktopWindowXamlSource.Content` and
releasing the source tears down the tree together. If a reusable service attaches
to arbitrary elements, remove `DragStarting` and `DropCompleted` handlers when
the service detaches.

Cancel or defer destructive host teardown while a source completion callback is
running. Do not create a second island drag manager during reparenting; the XAML
root owns its broker integration.

## Failure signatures

| Symptom | First discriminating check |
| --- | --- |
| `DragStarting` never runs | Put `CanDrag=true` on the exact hit-tested element, not only an ancestor whose child handles the press. |
| Text selection changes instead of dragging | Confirm the explicit handle received the press; do not start on the editor surface. |
| The whole control appears as the preview | Call `DragUI.SetContentFromDataPackage()` after adding the text format. |
| Drag fails only when elevated | XAML drag initiation is unsupported in an elevated process; change the deployment model rather than adding another manager. |
| Drop never runs | Confirm `AllowDrop`, a non-null hit-test background, a matching format, and `AcceptedOperation` during `DragOver`. |
| Source text disappears after a failed Move | Delete only after `DropCompleted` reports Move and the saved source range still validates. |
| Behavior differs in an island | Compare the same minimal source in an ordinary WinUI window before changing host input routing. |

## Validation matrix

Build gates:

- Release `win-x64`;
- Release `win-arm64`;
- Markdown, relative-link, and skill validation.

Automated x64 mouse gate:

```pwsh
./scripts/Invoke-TextDragUi.ps1 `
    -Configuration Release `
    -RuntimeIdentifier win-x64
```

The runner builds both projects, validates both direct-editor host topologies,
validates both canonical handles and empty-selection cancellation, closes every
process, and writes a bounded JSON report.

Remaining manual gates for each supported input device:

| Source | Target | Checks |
| --- | --- | --- |
| TextBox handle | Same-island TextBox target | Nonempty selection, empty-selection cancellation, Copy result, system preview. |
| RichEditBox handle | Same-island TextBox target | Multiline selection, empty-selection cancellation, Copy result, system preview. |
| Either handle | External standard text target | Interoperable text payload and Copy result. |
| External standard text source | XAML target | Format allowlist, payload cap, callback containment. |

Repeat after source replacement and host teardown. Re-run the direct TextBox and
RichEditBox diagnostic on every Windows App SDK upgrade.

## Sample

The [bundled raw-HWND host](assets/minimal-host/README.md) implements the
Copy-only explicit-handle pattern without `InputPointerSource`, routed pointer
handlers, manual capture, direct `StartDragAsync`, or a custom
`DragDropManager`.

The [direct editor diagnostic](assets/direct-editor-drag-repro/README.md) links
the same TextBlock, TextBox, and RichEditBox source content into an ordinary
WinUI Window and the raw island host. It is a reproduction asset, not a
recommended implementation.

## Related upstream issues

- [microsoft-ui-xaml#10681](https://github.com/microsoft/microsoft-ui-xaml/issues/10681)
  reports a draggable ancestor not initiating when child content owns input.
- [microsoft-ui-xaml#6022](https://github.com/microsoft/microsoft-ui-xaml/issues/6022)
  includes discussion steering a manual `StartDragAsync` call in
  `PointerMoved` toward the documented automatic path.
- [microsoft-ui-xaml#10144](https://github.com/microsoft/microsoft-ui-xaml/issues/10144)
  tracks custom drag visuals that do not appear.
- [microsoft-ui-xaml#7690](https://github.com/microsoft/microsoft-ui-xaml/issues/7690)
  tracks drag/drop failure in elevated WinUI processes.

## Sources

Use the drag/drop documentation, official sample, and pinned WinUI implementation
entries in [sources.md](sources.md). Recheck source observations whenever the
Windows App SDK package changes.

## Known gaps

- Direct TextBox and RichEditBox do not initiate on the measured x64 mouse
  baseline; behavior on other package versions and devices is not established.
- Touch and pen behavior has not been measured in the bundled host.
- The sample does not implement Move, rich-format transfer, virtual files, or
  promised data.
- Cross-integrity and elevated-process behavior needs a deployment-specific
  design rather than a sample workaround.
