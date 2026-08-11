# DPI and coordinate-space cookbook

This guide maps native HWND, XAML, composition, popup, and OLE coordinates for a
Per-Monitor V2 process. Most mixed-DPI defects are not rounding defects; they are
origin or unit mismatches.

## Applies to

- `DesktopWindowXamlSource` in a Per-Monitor V2 Win32 process.
- Windows App SDK 1.4 or later.
- Native window and OLE APIs that report physical pixels.
- XAML layout and pointer APIs that report view pixels relative to a named root or
  element.
- Multiple monitors, including negative virtual-screen coordinates.

The bundled minimal host is Per-Monitor V2 and resizes the site bridge. Its
automated RID-build gate does not exercise a real mixed-monitor transition.

## Vocabulary

A **raw pixel** is a physical device pixel. A **view pixel** is XAML's effective
layout unit, commonly called a DIP. Microsoft documents
`XamlRoot.RasterizationScale` as the number of raw pixels per view pixel.

$$
\text{raw pixels} = \text{view pixels} \times \text{RasterizationScale}
$$

$$
\text{view pixels} = \frac{\text{raw pixels}}{\text{RasterizationScale}}
$$

Do not call both values `pixels` in code or diagnostics.

## Coordinate table

| Source/API | Origin | Unit | Notes |
| --- | --- | --- | --- |
| `GetWindowRect` | Virtual-screen top-left | Raw physical pixels | Coordinates can be negative. |
| OLE `POINTL` | Virtual-screen top-left | Raw physical pixels | Convert through the actual target HWND/site bridge. |
| `ScreenToClient` / `ClientToScreen` | Named HWND client origin | Raw physical pixels in Per-Monitor V2 | Name the HWND in helper names. |
| Parent `WM_SIZE` client dimensions | Parent client origin | Raw physical pixels | Use for site-bridge bounds. |
| `DesktopSiteBridge.MoveAndResize` | Parent HWND client origin | Integer raw pixels | Position and size the bridge; do not pass XAML DIPs. |
| `XamlRoot.Size` / element layout | XAML root/parent | View pixels | Scale to compare with native size. |
| `XamlRoot.RasterizationScale` | Island | Raw pixels per view pixel | Query current root; do not cache process-wide. |
| `PointerRoutedEventArgs.GetCurrentPoint(element)` | Named XAML element | View pixels | Prefer over interpreting island-level positions. |
| `DragEventArgs.GetPosition(element)` | Named XAML element | View pixels | Correct target-relative drag point. |
| Composition child visual offset | Attached visual | View/composition units | Account for transform and clipping of the attachment point. |

## Site-bridge sizing

On creation and every relevant native resize:

```csharp
if (!PInvoke.GetClientRect(parentHwnd, out RECT client))
{
    throw new Win32Exception(Marshal.GetLastPInvokeError());
}

source.SiteBridge.MoveAndResize(new RectInt32(
    0,
    0,
    client.right - client.left,
    client.bottom - client.top));
```

When a wrapper child HWND occupies only part of the parent, use the wrapper's
parent-client physical rectangle. If the source was initialized against the wrapper
itself, the bridge rectangle normally begins at `(0, 0)` in wrapper-client space.

XAML controls receive logical/view dimensions inside the bridge. Do not multiply a
`Width=240` XAML element by the native DPI scale before assigning it.

## Screen physical point to XAML root point

Given an OLE or cursor point in virtual-screen physical pixels:

1. Obtain the current site-bridge HWND from `source.SiteBridge.WindowId` and
   `Win32Interop.GetWindowFromWindowId`.
2. Get its physical screen rectangle or call `ScreenToClient` on that exact HWND.
3. Divide the resulting bridge-client raw coordinates by the live
   `XamlRoot.RasterizationScale`.
4. If the target control does not fill the root, transform from root space to that
   element or use an API that accepts the target directly.

```csharp
Point rootViewPoint = new(
    bridgeClientRawX / xamlRoot.RasterizationScale,
    bridgeClientRawY / xamlRoot.RasterizationScale);
```

Subtracting a wrapper HWND origin while using a site-bridge scale is valid only if
their client origins coincide. State and test that invariant rather than assuming
it.

## XAML element point to screen physical point

1. Transform the element point into root view coordinates.
2. Multiply by the current root scale.
3. Round only at the boundary that requires integer native pixels.
4. Add the site-bridge HWND's physical screen origin.

For rectangles, transform both corners; scaling only width/height ignores
translation and can fail under transforms or right-to-left layout.

## Rounding

Use one rounding boundary. For a logical size converted to native pixels:

```csharp
int rawWidth = checked((int)Math.Round(
    viewWidth * xamlRoot.RasterizationScale,
    MidpointRounding.AwayFromZero));
```

Match the surrounding framework's established rounding policy. Allow a one-pixel
tolerance in diagnostic comparisons where independent layout engines round at
different boundaries. Do not repeatedly convert raw to view to raw during one
layout pass; that accumulates drift.

## Per-Monitor V2 transition

A top-level `WM_DPICHANGED` sequence should:

1. Read old/new DPI and the suggested top-level window rectangle.
2. Apply the suggested native rectangle according to framework policy.
3. Recompute native child layout in physical pixels.
4. Resize the site bridge to the current host client rectangle.
5. Let XAML update its root and layout.
6. Observe `XamlRoot.Changed` and query the new `RasterizationScale`.
7. Update diagnostics and popup/overlay state after both sides are current.

A child host can receive its own DPI notification depending on the native
framework. Make repeated resize calls idempotent. Do not require native and XAML
notifications to arrive in one fixed order.

## Live scale observation

Subscribe to `FrameworkElement.Loaded` before attaching to `XamlRoot.Changed`, and
unsubscribe on `Unloaded` or disposal. A content element can move to a replacement
source/root during reparenting.

```csharp
private void AttachRoot(FrameworkElement element)
{
    XamlRoot? root = element.XamlRoot;
    if (!ReferenceEquals(root, _subscribedRoot))
    {
        DetachRoot();
        _subscribedRoot = root;
        if (root is not null)
        {
            root.Changed += RootChanged;
        }
    }
}
```

Do not store one scale for the process or top-level window and apply it to every
island indefinitely.

## Negative virtual-screen coordinates

Monitors left of or above the primary monitor have negative screen coordinates.
Keep screen coordinates signed through subtraction and conversion. Packing into
unsigned low/high words is appropriate only for APIs whose documented message
format uses signed 16-bit coordinates, and it cannot represent all modern virtual
desktop positions.

Use `POINT`, `POINTL`, or 32-bit integer structures for cross-monitor geometry.

## Popups and overlays

A popup belongs to an island through `XamlRoot`. Open it from a live target element
or assign the root explicitly. Validate alignment after each DPI transition and
when the parent uses negative coordinates.

Composition child visuals use the coordinate space of the element to which they
are attached. A visual attached to the editor should use editor-relative view
coordinates, not OLE screen pixels or raw island coordinates.

Native sibling overlays are separate HWNDs. Their z-order is native and their
bounds are physical; XAML `Canvas.ZIndex` cannot order them relative to the
site-bridge HWND.

## OLE and drag coordinates

OLE `IDropTarget` receives a physical screen `POINTL`. Convert it through the
registered target HWND. If that HWND is the site bridge, use its origin. If an OLE
source reaches XAML's normal routed target, use `DragEventArgs.GetPosition(editor)`
and let XAML perform island hit testing and transforms.

Do not pass lower-level `DragInfo.Position` directly to an editor unless its origin
and units are established for that target. The routed API exists to provide a
specified relative point.

## Diagnostic display

Show all of these together while moving the window:

- native top-level DPI and scale percentage;
- top-level virtual-screen rectangle and client physical size;
- wrapper/site-bridge physical rectangle;
- `XamlRoot.RasterizationScale`;
- XAML root view size and computed raw size;
- one native and one XAML reference ruler with the same logical dimensions;
- DPI transition count and last old/new values.

A useful invariant is:

$$
\left|\text{host raw size} - \text{XAML view size} \times \text{scale}\right| \leq 1
$$

for a root intended to fill the host.

## Failure signatures

| Symptom | First check |
| --- | --- |
| Island grows or shrinks twice at non-100% DPI | XAML dimensions were manually scaled before XAML scaled them. |
| Pointer/caret offset is proportional to scale | Raw physical point was used as a view point or divided twice. |
| Constant offset independent of scale | Wrong HWND/root origin was subtracted. |
| Correct on primary, wrong on left monitor | Unsigned or truncated screen coordinates. |
| One blank frame during monitor move | Bridge/native resize and XAML root update are observed at different stages; inspect event order. |
| Popup opens on wrong monitor or offset | Missing/wrong `XamlRoot`, stale scale, or work-area policy. |
| Drift after repeated transitions | Repeated conversion/rounding or stale cached geometry. |
| Native overlay appears under XAML | HWND z-order issue, not a XAML coordinate issue. |

## Manual validation matrix

Configure real or virtual monitors with distinct Windows Scale values. Cover 100%,
125%, 150%, 200%, and 300% where the environment permits.

For each ordered pair of available scales:

1. Move the window fully from A to B and back.
2. Compare native DPI and XAML scale.
3. Check the native and XAML reference rulers.
4. Resize, maximize, restore, and snap.
5. Open a XAML popup and interact with it.
6. Repeat transitions at least ten times.
7. Include a monitor with negative virtual-screen coordinates when available.
8. Record blank frames, clipping, focus loss, popup offset, and size drift.

The current authoring environment did not execute this matrix. Keep the result
pending until a machine with the required monitor scales produces a retained
report.

## Automated validation

Automate what is deterministic without pretending a synthetic DPI message proves a
full monitor transition:

- build the host for every supported RID;
- assert bridge bounds follow native client bounds on resize;
- assert conversion helpers at negative coordinates and fractional scales;
- verify a fixed XAML view size multiplied by root scale matches host raw size
  within one pixel;
- retain screenshots/pixel samples for airspace and clipping scenarios;
- run the live transition matrix manually or in a controlled multi-display lab.

## Sources

Use `XamlRoot.RasterizationScale`, DPI-awareness, site-bridge, island design-note,
and native DPI links in [sources.md](sources.md). Pin implementation claims and
record the OS/build/display configuration with measured results.

## Known gaps

HDR scaling, remote desktop DPI virtualization, display hot-plug, rotation,
right-to-left transforms, accessibility text scaling, and windowless islands need
additional matrices when they enter product scope.
