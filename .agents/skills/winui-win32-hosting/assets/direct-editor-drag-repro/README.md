# Direct editor drag diagnostic

This buildable diagnostic compares direct `CanDrag` behavior for TextBlock,
TextBox, and RichEditBox without custom pointer handling.

It provides two host topologies:

- `DirectEditorDragRepro` places the content in an ordinary WinUI Window.
- `MinimalWinUIHost --direct-editor-repro` places the same linked content in a
  raw Win32 `DesktopWindowXamlSource` host.

TextBlock is the control case. TextBox and RichEditBox each offer their current
selection from `DragStarting`. Every source is Copy-only and uses the system
data-format visual.

The diagnostic deliberately contains no `InputPointerSource`, routed pointer
handlers, manual capture, movement threshold, direct `StartDragAsync`, custom
`DragDropManager`, or OLE source.

## Build

Build the ordinary Window diagnostic:

```pwsh
dotnet build DirectEditorDragRepro.csproj --configuration Release --runtime win-x64
dotnet build DirectEditorDragRepro.csproj --configuration Release --runtime win-arm64
```

Build the raw island host from the adjacent `minimal-host` directory:

```pwsh
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-x64
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-arm64
```

Building establishes only API and architecture compatibility.

## Automated comparison

From this directory:

```pwsh
../../scripts/Invoke-TextDragUi.ps1 `
  -Configuration Release `
  -RuntimeIdentifier win-x64
```

The runner retains a JSON result, requests top-level window closure, and kills a
process if it does not exit within the bounded timeout. On the measured Windows
App SDK 2.3.1, .NET 10.0.9, Windows build 26200, non-elevated x64 mouse run:

| Host | Source | Result |
| --- | --- | --- |
| WinUI Window | TextBlock | `DragStarting`, Copy, exact target text |
| WinUI Window | TextBox | No `DragStarting`; selection gesture |
| WinUI Window | RichEditBox | No `DragStarting`; selection gesture |
| `DesktopWindowXamlSource` | TextBlock | `DragStarting`, Copy, exact target text |
| `DesktopWindowXamlSource` | TextBox | No `DragStarting`; selection gesture |
| `DesktopWindowXamlSource` | RichEditBox | No `DragStarting`; selection gesture |

This result distinguishes text-control handling from island hosting. It does not
generalize to touch, pen, ARM64 execution, elevation, or another package version.

## Manual comparison

Run the ordinary Window case:

```pwsh
dotnet run --project DirectEditorDragRepro.csproj --configuration Release --runtime win-x64
```

Run the raw island case from `minimal-host`:

```pwsh
dotnet run --project MinimalWinUIHost.csproj --configuration Release --runtime win-x64 -- --direct-editor-repro
```

For each host:

1. Drag the TextBlock control case to the Copy target.
2. Select TextBox text and attempt to drag from the selected text.
3. Select RichEditBox text and attempt the same gesture.
4. Record whether `DragStarting` and `DropCompleted` appear in the status.
5. Record final source selection, source text, target text, and preview shape.

Name the Windows App SDK package, OS build, process architecture, input device,
and elevation state with every result. An intermittent pass is not evidence of a
supported product contract.

The measured direct editors fail while TextBlock succeeds. Preserve this
diagnostic as the upstream reproduction and do not add a pointer workaround to
the canonical sample.
