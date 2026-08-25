# Minimal WinUI 3 island host

This code-only .NET sample owns a raw Win32 HWND and message loop, then hosts
WinUI 3 content with `DesktopWindowXamlSource`. It is pinned to Windows App SDK
2.3.1 and targets Windows 10 version 1809 or later.

The hosted content demonstrates the canonical Copy-only text drag source:

- TextBox and RichEditBox own editing and selection.
- A separate `CanDrag` handle beside each editor owns the gesture.
- `DragStarting` offers the selected text and requests a system data-format
  visual.
- A routed XAML TextBox target accepts and inserts copied text.

The sample does not put `CanDrag` directly on an editable control and does not
use `InputPointerSource`, routed pointer handlers, manual capture,
`StartDragAsync`, a custom `DragDropManager`, or OLE registration.

Build both documented architectures:

```pwsh
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-x64
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-arm64
```

Run the x64 sample interactively:

```pwsh
dotnet run --project MinimalWinUIHost.csproj --configuration Release --runtime win-x64
```

The machine must have the matching Windows App Runtime and Visual C++
Redistributable installed.

Manual checks:

1. Select text in the TextBox and drag its adjacent handle to the target.
2. Repeat with a multiline RichEditBox selection.
3. Confirm an empty selection cancels before a drag starts.
4. Confirm the preview represents the text data format, not the editor control.
5. Confirm `DropCompleted` reports Copy and the source remains unchanged.
6. Repeat resize, focus, and shutdown checks from the base host walkthrough.

The x64 mouse cases above are also automated:

```pwsh
../../scripts/Invoke-TextDragUi.ps1 `
  -Configuration Release `
  -RuntimeIdentifier win-x64
```

The measured TextBox and RichEditBox handles both completed with Copy, retained
their source text and full selection, and inserted exact text into the target.
The empty-selection case canceled and retained the target sentinel. The system
preview showed a document-format icon and Copy glyph rather than an editor
snapshot.

Direct TextBox and RichEditBox source initiation, Move, touch, pen, rich-format
payloads, virtual files, and elevated processes are outside this sample.

See [XAML text drag sources](../../xaml-drag-source.md) for the API contract,
evidence boundary, and failure signatures.

The separate [direct-editor diagnostic](../direct-editor-drag-repro/README.md)
uses the same direct `CanDrag` content in an ordinary WinUI Window and in this
raw host. Its measured result explains why this canonical sample uses handles.
