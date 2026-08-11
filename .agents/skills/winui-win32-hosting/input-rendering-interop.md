# Input, rendering, accessibility, and interop

Use this page after the source can display content. These boundaries are where a
host can look correct while still being unusable.

Use [message and focus routing](message-and-focus-routing.md) for the complete Tab
algorithm and [DPI and coordinate spaces](dpi-and-coordinate-spaces.md) for unit
and origin conversions. Use [island pointer and cursor behavior](island-pointer-and-cursor.md)
for the full pointer state machine and [mixed OLE and XAML drag/drop](mixed-ole-and-xaml-drag-drop.md)
for transport, target ownership, reentrancy, and editable-text transactions. Use
[popup, airspace, and z-order](popup-airspace-and-z-order.md) for visual ownership
and pixel oracles, and [accessibility across islands](accessibility-across-islands.md)
for UI Automation and assistive-technology validation.

## Applies to

- HWND-backed WinUI islands in Windows App SDK 1.4 or later.
- Native/XAML keyboard, pointer, focus, DPI, popup, airspace, accessibility, and
  drag/drop boundaries.
- Routed XAML APIs by default, with lower-level `ContentIsland` or OLE APIs only
  when the application intentionally owns that boundary.
- Real-window integration validation; unit-only behavior is not treated as proof
  of OS registration, routing, or composition.

## Message preprocessing

A custom Win32 loop must give Windows App SDK content the first opportunity to
process each retrieved message:

```csharp
while (GetMessage(out MSG message, default, 0, 0))
{
    if (ContentPreTranslateMessage(&message))
    {
        continue;
    }

    TranslateMessage(message);
    DispatchMessage(message);
}
```

Declare the `Microsoft.UI.Windowing.Core.dll` export with a blittable `MSG*`
signature when no supported projection exposes it. Preserve native `BOOL` width.
Do not run it after XAML/framework shutdown.

Pretranslation handles XAML keyboard and island input, but a mixed native/XAML
dialog still needs explicit Tab-direction routing at the boundary.

## Focus and Tab navigation

From Win32's perspective, the island is one child HWND. XAML knows only the
focusable elements inside its own tree. Stitch the two models together:

- On forward Tab into the island, call `NavigateFocus(First)`.
- On Shift+Tab into the island, call `NavigateFocus(Last)`.
- Handle `TakeFocusRequested` and move to the next or previous native sibling.
- Use native dialog traversal such as `GetNextDlgTabItem` or the owning
  framework's equivalent for native controls.
- When `NavigateFocus` reports no move, continue native traversal rather than
  trapping focus.
- Track navigation correlation IDs. A request can synchronously raise
  `TakeFocusRequested` when no focusable XAML element exists.
- Avoid reactivating another top-level window merely because XAML's logical focus
  changed on a shared thread.

Test forward and reverse traversal across at least native-before, multiple XAML
stops, hosted HWND content such as WebView2 if present, and native-after.

## Pointer and cursor input

See [island pointer and cursor behavior](island-pointer-and-cursor.md) before
subscribing below XAML's routed-event layer.

Use routed XAML pointer events for ordinary controls. Use
`InputPointerSource.GetForIsland(contentIsland)` only when the host must observe
input before or outside XAML element routing. Acquire it after the element has a
live `XamlRoot.ContentIsland`; unsubscribe and drop the cached reference when that
island unloads.

Text controls and other class handlers may mark pointer events handled and capture
the pointer. Register routed handlers with `handledEventsToo` when observation is
required without replacing built-in selection, click, or capture behavior.
Keep pointer IDs and button state; reset on release, cancellation, routed-away,
and capture loss.

Cursor ownership can change after class handling. If the application overrides an
island cursor, do it at a point in routing where the control cannot immediately
replace it, preserve the original cursor, and dispose created `InputCursor`
objects.

## Coordinate spaces and DPI

Never name a variable merely `point` at a framework boundary. Record both origin
and unit.

| Source | Typical unit/origin |
| --- | --- |
| Win32 `POINT`, OLE `POINTL`, `GetWindowRect` | Physical screen pixels. |
| Native child-client coordinates | Physical pixels relative to that HWND. |
| `DesktopChildSiteBridge.MoveAndResize` | Integer parent-client rectangle. |
| XAML pointer and layout APIs | Effective pixels relative to a named element or island. |
| Composition visual offsets | Effective coordinates in the attached visual's space. |

Use a Per-Monitor V2 manifest. On a DPI change, apply the suggested native window
rectangle according to the host framework, resize the site bridge, then query the
current `XamlRoot.RasterizationScale`. Do not cache one system DPI for the process.
Test movement in both directions across monitors with different scales and include
negative desktop coordinates.

For XAML drag events, prefer `DragEventArgs.GetPosition(target)` over manually
interpreting a lower-level island position.

## Theme and resources

Merge `XamlControlsResources` before creating controls that depend on WinUI theme
resources. Propagate the native application's light/dark/system mode through the
host root's `RequestedTheme`, but respect an explicit theme set by the content
owner. Reapply an application-owned theme when the native color mode changes.

High Contrast is not equivalent to a dark palette. Validate it with the real
system mode and preserve WinUI's theme-resource behavior. Do not replace dynamic
resources with hard-coded colors merely to make one screenshot match.

## Popup and airspace behavior

See [popup, airspace, and z-order](popup-airspace-and-z-order.md) for the complete
ownership, clipping, native ordering, screenshot, and popup-edge recipe.

A popup, flyout, menu, or dialog must know its island. When it cannot infer the
owner from a live target element, set its `XamlRoot` explicitly. Decide whether
`ShouldConstrainPopupsToWorkArea` matches the host's window-management policy.

HWND-hosted XAML participates in native airspace:

- The site bridge owns a child HWND.
- Native and XAML siblings overlap according to HWND z-order, not XAML `Canvas.ZIndex`.
- Use `WS_CLIPCHILDREN` and `WS_CLIPSIBLINGS` where appropriate.
- Coordinate z-order with `SetWindowPos` and `SWP_NOACTIVATE` so visual fixes do
  not steal focus.
- Parent clipping still applies when a child starts at a negative client
  coordinate or extends outside the viewport.

Prove overlap and clipping with screenshots and pixel samples, not only HWND order.
Include popups because they may use a separate bridge or compositor path.

## Accessibility

See [accessibility across islands](accessibility-across-islands.md) for bounded
external capture, pattern/focus assertions, and the manual Narrator, High Contrast,
text-scale, and magnifier matrix.

`DesktopWindowXamlSource` and its site bridge connect the XAML UI Automation
fragment to the native window hierarchy. Validate the combined result from the
outside process:

- one expected top-level root;
- parent indices or ancestry that cross the native/island boundary correctly;
- stable, unique runtime IDs within one capture;
- names and control types for both native and XAML elements;
- required patterns such as Invoke, Value, RangeValue, and Text;
- focused-element state after native-to-XAML and XAML-to-native traversal.

Validate window ownership before capturing UIA or screenshots from a reported
HWND. Bound traversal depth, element counts, text, output, and capture duration.
UIA snapshots do not replace Narrator, keyboard-only, High Contrast, text-scale,
or screen-magnifier checks.

## Drag and drop

See [mixed OLE and XAML drag/drop](mixed-ole-and-xaml-drag-drop.md) for the full
ownership, lifecycle, text-edit transaction, and validation recipe.

### Choose the correct layer

For ordinary XAML content:

- Initiate a custom gesture with `UIElement.StartDragAsync` and populate the
  `DataPackage` in `DragStarting`.
- Set `AllowDrop` and handle routed `DragEnter`, `DragOver`, `DragLeave`, and
  `Drop` events.
- Use `DragEventArgs.GetPosition(target)` for target-relative coordinates.

At the WinUI source commit pinned in [sources.md](sources.md),
`UIElement.StartDragAsync` adapts to
`Microsoft.UI.Input.DragDrop.DragOperation.StartAsync` using the manager retained
by the island root. `CXamlIslandRoot` registers one `TargetRequested` handler for
XAML and removes it on disconnect. This is source-observed architecture, not a
public extension contract. Do not acquire another manager merely to customize
ordinary XAML hit testing or routed events.

Do not call `RegisterDragDrop` on the XAML-owned site-bridge HWND. That collides
with XAML's target ownership; use routed XAML events for that surface. Register
classic OLE only on a separate, application-owned native target HWND.

Use `DragDropManager` and `IDropOperationTarget` directly only when implementing a
custom content-island framework layer that intentionally owns target routing.

### Mixed Win32 and WinUI behavior

The Windows Runtime drag broker interoperates with system drag/drop, but its
source UI is not identical to classic OLE. Target-side `DragUIOverride.Clear`
hides content, glyph, and caption only while a cooperating WinUI target owns the
override. `DragUIContentMode` selects synchronous versus deferred content; it is
not a source-side no-UI mode.

If a product requires identical classic cursor feedback over native targets, an
OLE `DoDragDrop` source may be the correct transport even when gesture detection
starts in XAML. Keep the XAML target on normal routed events unless the application
really owns the island framework.

OLE requirements include:

- STA ownership and balanced `OleInitialize`/`OleUninitialize` policy;
- synchronous `DoDragDrop`, which runs a nested message loop;
- correct `IDataObject` formats, `FORMATETC`, `STGMEDIUM`, allocator, and
  `ReleaseStgMedium` ownership;
- `IDropSource.QueryContinueDrag` and feedback HRESULTs;
- `RegisterDragDrop`/`RevokeDragDrop` against the application-owned native
  hit-test HWND, not an obscured wrapper or XAML-owned site bridge;
- revocation before source replacement, parent destruction, or dispatcher
  shutdown;
- callback data that cannot escape its documented lifetime.

### Editable text behavior

WinUI `TextBox` and `RichEditBox` do not expose built-in Word/Notepad-style
selected-text dragging. WinUI source disables RichEdit's native drag path and does
not implement its native drop-caret callback. Application behavior must:

1. Snapshot a nonempty selected range before the control collapses it.
2. Start only from a press inside that range.
3. Preserve normal click, double-click, and selection behavior outside the range.
4. Preserve the source selection during dragging.
5. Negotiate Move by default and Copy with Ctrl when allowed.
6. Reject a same-control move whose insertion point lies inside the source range.
7. Insert and select the destination text, then delete the original only after a
   successful Move, adjusting indices for same-control moves.
8. Restore source selection and focus after cancellation or failed drop.
9. Draw target insertion feedback separately.

A composition child visual is a supported caret surface:
`ElementCompositionPreview.SetElementChildVisual` places it above the attached
XAML element. Use a theme-resolved opaque brush, target-relative coordinates,
pixel snapping, and cleanup on leave/drop/unload. A XAML overlay panel is an
alternative when changing the hosted visual tree is acceptable.

Do not use Windows Notepad as proof of hidden WinUI text behavior. Modern Notepad
uses a WinUI shell around a native `RichEditD2DPT` editor with private OLE
integration.

## Security and resilience

Treat external drag data, XAML metadata, resource keys, HWNDs, and subprocess
protocols as untrusted inputs. Bound allocations and enumeration, use checked
size arithmetic, validate HWND process/thread ownership, and translate exceptions
at COM/native callbacks. Run the security-review workflow for custom COM, unsafe
memory, drag/drop data, or caller-supplied markup.
