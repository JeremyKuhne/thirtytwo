# Mixed OLE and XAML drag/drop

Use this guide when a Win32 host must move data between native HWND content and
WinUI controls in a `DesktopWindowXamlSource`. Keep gesture detection, transport,
target routing, visual feedback, and data mutation as separate decisions.

## Applies to

- Windows App SDK 1.7 or later, framework-dependent unpackaged applications on
  Windows 10 version 1809 or later.
- .NET 10 on an STA thread with an HWND-backed WinUI island.
- Routed XAML drag/drop, the Windows Runtime drag broker, and classic OLE
  `DoDragDrop` interoperability.
- XAML-only source behavior measured with x64 mouse input in the bundled assets;
  the consuming framework's XAML target configuration and lifecycle are tested,
  and native OLE input reaches routed XAML `DragEnter` with text plus Copy and
  Move exposed. The measured empty TextBox target rejected that entry while its
  wrapper requested unsupported character geometry. The empty-target regression
  passes. A subsequent native run accepted Move but timed out before an observable
  Drop. The next run proved mouse-up injection returned and routed TextBox `Drop`
  ran with Copy and Move allowed, no modifiers, and final acceptance Move; it then
  timed out before target commit or source deletion. A retrieval-traced run then
  recorded product Drop entry, deferral acquisition, `GetTextAsync` start, and
  native `QueryGetData` and `GetData` entry and return on the main STA.
  `GetTextAsync` did not complete. The artifact did not record the `GetData`
  HRESULT, so do not call the extraction successful from that run alone. Those
  events came from temporary product observers that were removed after the
  investigation. No mixed native/XAML transfer has therefore been recorded.
  ARM64 execution and all remaining mixed-transfer behavior remain manual gates.
- API and source-observed behavior checked against Windows App SDK 2.3.1.

The drag/drop primitives described here begin in Windows App SDK 1.4, but the
`XamlRoot.ContentIsland` access used by this host recipe begins in 1.7.

The [bundled minimal host](assets/minimal-host/README.md) implements a Copy-only
XAML text source and target with explicit drag handles. Use
[xaml-drag-source.md](xaml-drag-source.md) for that canonical baseline before
adding a mixed native/XAML direction.

## Choose one target owner

| Scenario | Source | Target |
| --- | --- | --- |
| Ordinary WinUI controls | `CanDrag` plus `DragStarting`; `StartDragAsync` only for an application-owned custom gesture | `AllowDrop` and routed XAML drag events |
| Native source into ordinary WinUI | OLE or another system drag source | Routed XAML drag events through the platform bridge |
| WinUI source into native HWNDs | XAML drag for standard system transfer; OLE when classic source feedback is required | Native `IDropTarget` |
| Custom island framework | `DragOperation` | One `DragDropManager.TargetRequested` owner and `IDropOperationTarget` |
| Native-only HWND surface | OLE `DoDragDrop` | OLE `RegisterDragDrop` and `IDropTarget` |

At the source commit pinned in [sources.md](sources.md), `CXamlIslandRoot`
acquires a `DragDropManager` when its island connects, installs one
`TargetRequested` handler, and removes that handler when the island disconnects.
`UIElement.StartDragAsync` retrieves the manager retained by that root. This is
source-observed behavior, not a public extension point or documented restriction
on `GetForIsland`. The ownership guidance below is inferred from that architecture
and must be rechecked when the package changes. Do not acquire another manager
merely to customize ordinary XAML content. A custom manager is appropriate when
the application intentionally owns target routing for a non-XAML island framework.

## Topology and ownership

```mermaid
flowchart LR
    Gesture[Pointer gesture] --> Source{Source transport}
    Source -->|XAML| Broker[Windows Runtime drag broker]
    Source -->|OLE| Loop[DoDragDrop nested loop]
    Broker --> Bridge[Desktop site-bridge HWND]
    Loop --> Bridge
    Bridge --> Owner{Target owner}
    Owner -->|Ordinary XAML| Routed[DragEnter / DragOver / Drop]
    Owner -->|Custom island layer| Operation[IDropOperationTarget]
    Owner -->|Native HWND| OleTarget[IDropTarget]
```

| Resource | Owner | Required cleanup |
| --- | --- | --- |
| Source `DataPackage` | Drag operation | Do not retain callback-only views beyond their contract. |
| OLE `IDataObject*` received by a callback | Caller for callback duration unless explicitly AddRef'd | Release any acquired reference and every owned `STGMEDIUM`. |
| Native drop registration | HWND owner thread | Call `RevokeDragDrop` before handle destruction or replacement. |
| `DragDropManager` event subscription | Custom island framework | Remove the handler when the `ContentIsland` unloads or changes. |
| Drop feedback visual | Target content | Remove it on leave, drop, unload, cancellation, and failure. |

Registration follows identity, not wrapper-object lifetime. Reparenting that
replaces either the native target HWND or the `ContentIsland` requires revoking
the old registration and binding the replacement after it is live.

## Routed XAML path

For ordinary content:

1. Set `CanDrag=true` on the element that owns the ordinary XAML gesture.
2. Populate `DragStartingEventArgs.Data` and set `RequestedOperation`.
3. Set `AllowedOperations` to the complete source effect set.
4. Set `AllowDrop=true` on the target.
5. Set `AcceptedOperation` during `DragOver`.
6. Read `DataView` asynchronously during `Drop`.
7. Use `DragEventArgs.GetPosition(target)` for target-relative feedback.

Do not reinterpret an island-level position as an element-relative point. The
target element may be translated, scrolled, mirrored, or scaled inside the root.
Give an `AllowDrop` target a non-null background, using `Transparent` when it
should receive pointer hit testing without visible fill.

`UIElement.StartDragAsync` is not supported in an elevated process. Treat process
elevation as a deployment constraint, not as an input bug to repair with a second
island manager.

Do not call `StartDragAsync` from every routed `PointerMoved` event. The
`CanDrag` path already owns capture, system thresholds, mouse, pen, touch,
release, and capture loss. Use the lower-level method only when the application
owns a genuinely custom gesture.

For editable TextBox or RichEditBox content, prefer a separate `CanDrag` handle
that reads the editor selection in `DragStarting`. Directly setting `CanDrag` on
the editable surface is not established by the bundled evidence and competes
with text-control class handling. Do not compensate with island pointer
observers unless a minimal platform reproduction first demonstrates that a
custom gesture is required.

## Classic OLE path

Classic OLE is appropriate when native targets require conventional source
feedback or when the application already owns an `IDataObject`/`IDropSource`
pipeline.

1. Balance every successful `OleInitialize` result (`S_OK` or `S_FALSE`) with `OleUninitialize`.
2. Build `FORMATETC` and `STGMEDIUM` values with explicit format, aspect, index,
   tymed, allocator, and release ownership.
3. Call `DoDragDrop` on the STA owner thread.
4. Treat the call as synchronous but reentrant: it runs a nested message loop.
5. Complete a move only when the returned effect is Move and the destination
   commit succeeded.

Classic OLE data extraction is synchronous unless the source and target negotiate
background extraction through the optional `IDataObjectAsyncCapability`. Absence
of that interface does not by itself violate the OLE contract. On the XAML side,
`DataPackageView.GetTextAsync` is a remote asynchronous operation, and a routed
Drop deferral keeps WinUI's Drop operation incomplete until the deferral is
completed. If routed `Drop` runs but mutation does not, trace entry and completion
around `GetTextAsync`, deferral completion, and native `IDataObject.QueryGetData`
and `GetData` with test-owned adapters or external diagnostics before changing
apartment or asynchronous-transfer behavior.

Every COM callback must translate managed exceptions to an HRESULT. Do not let an
exception cross the unmanaged boundary. Bound external format enumeration and
payload sizes before allocating or decoding.

`RegisterDragDrop` requires OLE initialization and a message-pumping owner thread;
initializing only with `CoInitialize`/`CoInitializeEx` is insufficient. The call
adds a reference to the target, and `RevokeDragDrop` releases it.

Register an OLE target only on an HWND the application owns as a native target.
It must be the window that receives drag hit testing, not an obscured wrapper or
top-level parent. Do not register over the site-bridge HWND already owned by XAML's
drag manager; `DRAGDROP_E_ALREADYREGISTERED` is an ownership collision, not a cue
to replace the existing target. Use routed XAML events for that surface or create a
separate native child target with explicit z-order and bounds. Validate a native
candidate with `IsWindow`, its owning process and thread, and the current host
generation before registration.

Classic `DoDragDrop` does not directly support initiation from touch or pen
handlers. Follow its documented synthesized-mouse path when classic OLE is the
required transport; prefer XAML drag APIs for native touch/pen gestures.

## Nested-loop rules

`DoDragDrop` does not return until drop or cancellation. During that interval:

- window messages and COM calls can reenter application code;
- the source selection, source HWND, island, and dispatcher can be invalidated;
- teardown must request cancellation or defer destructive work rather than
  releasing callback state underneath OLE;
- completion code must revalidate every object captured before the call;
- no lock needed by a callback may be held across `DoDragDrop`.

Model the drag session as one state transition with one terminal completion. Late
leave, capture-loss, or cancellation notifications must not commit or clean up a
second time.

At the pinned WinUI source commit, `DropOperationTarget` queues drag callbacks
that reenter while a prior callback is reading cross-process `DragInfo`. It also
tracks the active target per thread because leave from one island is not guaranteed
to precede enter/over for the next. A custom target must not assume perfectly
nested enter/leave ordering across islands; make enter replace stale target state
and make a late leave idempotent.

## Editable-text move transaction

WinUI `TextBox` and `RichEditBox` do not expose a complete native-editor selected
text drag contract. Application code must preserve editing semantics explicitly:

1. Snapshot a nonempty selection and the source generation.
2. Begin only when the press is inside that selection.
3. Preserve ordinary click, double-click, and selection behavior elsewhere.
4. Offer Move by default and Copy when the modifier policy permits it.
5. Reject a same-control move into the source range.
6. Insert and select the destination text first.
7. Delete the original only after a successful Move; adjust source indices when
   insertion occurred before the original range.
8. Restore selection and focus after cancellation or failed commit.

For cross-control moves, the target commit and source deletion are separate
operations. Record enough source state to detect intervening edits before deleting
text. If the source generation changed, keep the inserted copy and do not delete
an unverified range.

The bundled source sample intentionally offers Copy only. Treat Move as a
separate feature after source initiation is reliable; do not add transaction
state merely to prove that a drag can start.

## Target feedback

Keep transport feedback separate from editor feedback. `DragUIOverride.Clear`
controls broker-owned content, glyph, and caption while a cooperating target owns
the override; it does not suppress every source visual over native targets.

A composition child visual can draw a text insertion caret without modifying the
editor's XAML tree. Resolve its brush from the active theme, use target-relative
view coordinates, snap physical edges with the live rasterization scale, and
remove the visual on every terminal path.

## Lifecycle sequence

```mermaid
stateDiagram-v2
    [*] --> Detached
    Detached --> Registered: HWND or ContentIsland becomes live
    Registered --> Dragging: source operation starts
    Dragging --> Registered: drop or cancellation completes
    Registered --> Detached: unload or source change
    Detached --> Registered: replacement identity becomes live
    Detached --> [*]: host disposal
```

For native reparenting, revoke the old HWND before changing it. For XAML source
replacement, detach the manager and feedback visual before clearing content, then
reacquire from the replacement `XamlRoot.ContentIsland` after load.

## Security and callback safety

Treat every incoming data object as untrusted, including drags originating in the
same process. Before reading a format:

- allowlist the format, `DVASPECT`, `lindex`, and `TYMED` combinations the target
  supports;
- cap format enumeration, item count, stream length, decoded text length, and
  total allocation;
- validate `HGLOBAL` size before locking or scanning it, and keep byte/character
  arithmetic checked;
- release every successfully returned `STGMEDIUM` exactly once with
  `ReleaseStgMedium`;
- copy data needed after a callback, or explicitly `AddRef` a retained COM
  interface and release it on every terminal path;
- reject stale HWND, island, source-generation, and target-generation values
  before mutation.

Initialize an outgoing effect to None before invoking application code. Native
callbacks validate required pointers and catch all managed exceptions, reset
session state, and return an appropriate failure HRESULT. WinRT async target
methods return `DataPackageOperation.None` after contained failures. Do not expose
exception text or arbitrary external payloads through an unbounded diagnostic
channel.

## Failure signatures

| Symptom | First discriminating check |
| --- | --- |
| XAML `Drop` never runs | Confirm `AllowDrop`, accepted operation, and that no custom manager replaced XAML's target owner. |
| XAML `Drop` runs but text is not committed | Trace `GetTextAsync` start/completion, Drop deferral completion, and native `QueryGetData` / `GetData`; do not infer which boundary stalled from routed Drop alone. |
| Native target never activates | Compare the registered HWND with the site-bridge HWND under the pointer. |
| Drag works until reparenting | Log registration HWND and `ContentIsland` identity before and after replacement. |
| Move duplicates or deletes wrong text | Log destination commit, returned effect, source generation, and adjusted deletion range. |
| UI freezes during drag | Find a lock held across `DoDragDrop` or work synchronously performed by a callback. |
| Crash during shutdown | Check whether registration, callbacks, or feedback outlived the HWND/island environment. |
| Caret offset changes with DPI | Compare `GetPosition(target)`, target transform, and live `RasterizationScale`. |
| Source visual differs over native targets | Determine whether the active transport is XAML broker drag or classic OLE. |
| Memory rises with malformed data | Log format/item/byte limits and verify every acquired medium or interface is released. |

## Validation matrix

Run each supported direction with Copy, Move, cancellation, invalid data, and
teardown where applicable:

| Source | Target | Required checks |
| --- | --- | --- |
| WinUI text | WinUI text, same island | Selection preservation, overlap rejection, Copy/Move, caret. |
| WinUI text | WinUI text, another island | Island identity, coordinates, source deletion after commit. |
| Native text | WinUI text | OLE-to-XAML bridge, format bounds, target-relative point. |
| WinUI text | Native text | Native target feedback, returned effect, cancellation. |
| Native text | Native text | Baseline OLE ownership and reentrancy. |

Repeat after reparenting and at 100%, 150%, and 200% display scale. Add parent
destruction during an active drag in a subprocess with a timeout and process-tree
cleanup. Retain lifecycle events and final source/target text for failures.

The portable skill has no bundled end-to-end mixed-transfer harness. Treat the
matrix as pending until a consuming framework records real-window results.

## Sources

Use the drag/drop, content-island, OLE, and WinUI implementation entries in
[sources.md](sources.md). Implementation observations must remain pinned to the
source commit recorded there.

## Known gaps

Touch/pen source gestures, shell virtual files, promised data, cross-integrity
drags, accessibility announcements, remote desktop, and drag-image parity across
native and XAML targets need dedicated validation before prescriptive claims.
