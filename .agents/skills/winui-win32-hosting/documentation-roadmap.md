# Documentation roadmap

Use this page to turn investigation results into durable technical documentation.
The current information is fragmented across API reference, deployment articles,
WinUI design notes, samples, and source. The missing artifact is an end-to-end,
versioned .NET Win32 hosting guide that connects those layers and proves its code.

## Documentation principles

Each document must:

- name the Windows App SDK version range and deployment model it covers;
- distinguish supported API, source-observed behavior, measured behavior, and
  future design;
- include a minimal compilable sample or link to a build-tested sample;
- state object, thread, HWND, coordinate-space, and cleanup ownership;
- include common failure signatures and the cheapest discriminating check;
- list automated and manual validation, including architectures not tested;
- link to exact official sources and pin implementation claims to commits;
- avoid absolute guarantees unless the invariant is stated and tested.

## Priority 0: make first hosting successful

### [End-to-end .NET Win32 host walkthrough](end-to-end-walkthrough.md)

**Status:** Implemented with a bundled code-only sample. Release x64 and ARM64
builds pass. Interactive startup, input, resize, clean shutdown, and clean-machine
deployment remain acceptance gates.

**Gap:** Existing material provides API pages and a C++ sample, but not one concise
code-only .NET walkthrough for an existing Win32 message loop.

**Content:** Project file, manifest, unpackaged bootstrap, STA entry point,
`DispatcherQueueController`, custom `Application`, metadata/resources,
`WindowsXamlManager`, HWND creation, `DesktopWindowXamlSource`, site-bridge sizing,
pretranslation, focus, disposal, and queue shutdown.

**Acceptance:** The published sample builds from CLI for x64 and ARM64, runs on a
clean machine, accepts keyboard/pointer input, resizes, and exits cleanly.

### [Host topology and ownership state machine](host-topology-and-ownership.md)

**Status:** Implemented with object, state, and sequence diagrams plus terminal
state tests for consuming frameworks. The bundled sample proves initialization,
normal close, and disposal through its implementation; rollback and reparenting
remain consumer integration gates.

**Gap:** API reference does not show parent HWND, wrapper HWND, site-bridge HWND,
`DesktopWindowXamlSource`, `DesktopChildSiteBridge`, `ContentIsland`, `XamlRoot`,
queue, and `Application` ownership together.

**Content:** A diagram plus initialization, normal close, parent destruction,
partial-construction rollback, dispatcher shutdown, and reparenting state machines.

**Acceptance:** Every arrow names ownership, thread, unit/origin, and disposal
responsibility; tests cover every terminal state.

### [Message and focus routing cookbook](message-and-focus-routing.md)

**Status:** Implemented with message ordering, entry/exit algorithms, correlation
handling, multiple-island and HWND-descendant rules, and an explicit automation
sequence. The bundled sample implements pretranslation and initial entry; full
mixed native/XAML traversal remains a consuming-framework gate.

**Gap:** `ContentPreTranslateMessage`, native dialog navigation, `NavigateFocus`,
`TakeFocusRequested`, correlation IDs, and reverse traversal are documented in
different places.

**Content:** Message-loop ordering; Tab/Shift+Tab algorithm; multiple islands;
WebView2/hosted HWND considerations; inactive top-level windows; focus-loop
prevention.

**Acceptance:** Automated traversal crosses native-before, several XAML stops,
and native-after in both directions without changing activation unexpectedly.

### [DPI and coordinate-space cookbook](dpi-and-coordinate-spaces.md)

**Status:** Implemented with origin/unit tables, conversion formulas, Per-Monitor
V2 sequencing, OLE/popup/composition boundaries, diagnostics, and automated/manual
matrices. The multi-monitor 100%-300% matrix remains pending suitable hardware or
virtual displays.

**Gap:** No single guide maps physical screen pixels, parent-client pixels,
site-bridge coordinates, XAML effective pixels, element-relative points, and
composition offsets.

**Content:** Conversion formulas, origins, `RasterizationScale`, Per-Monitor V2,
negative coordinates, `WM_DPICHANGED`, popup position, OLE points, and diagrams.

**Acceptance:** A matrix covers 100%, 125%, 150%, 200%, and 300% where hardware or
virtual displays allow, in both monitor directions.

## Priority 1: make the host production-ready

### Metadata and resource composition

Document one process `Application`, generated and manual `IXamlMetadataProvider`
registration, `XamlControlsResources`, library dictionaries, collision precedence,
and failure diagnosis. Include two libraries with intentional type and resource
collisions.

### [Popup, airspace, and z-order](popup-airspace-and-z-order.md)

**Status:** Guide written with root association, work-area policy, native HWND
ordering, parent/sibling clipping, signed coordinates, source replacement,
screenshot pixel oracles, failure signatures, and validation matrices. Native
sibling order and parent clipping were measured on x64 in a consuming framework.
The portable skill has no bundled popup/airspace harness; popup-edge, mixed-DPI,
ARM64, and assistive-magnification matrices remain pending.

**Gap:** No bundled portable scene proves both native sibling orders, parent-edge
clipping, popup-root/work-area policy, source replacement, and mixed-monitor
placement with retained pixels and interaction evidence.

**Content:** `XamlRoot` assignment, work-area constraints, site-bridge child HWNDs,
native sibling z-order, clipping, negative positions, popup bridges, and why XAML
`ZIndex` cannot order separate HWNDs. Back it with screenshot pixel oracles.

**Acceptance (pending):** Known-color screenshot assertions and interaction checks
cover both sibling orders, parent clipping, every shipped popup type, work-area
edges, source replacement, 100%-300% DPI pairs, keyboard/focus return, High
Contrast, Narrator, and magnifier.

### [Accessibility across the island boundary](accessibility-across-islands.md)

**Status:** Guide written with provider topology, semantic properties, custom-peer
boundaries, external bounded Control View capture, runtime-ID scope, pattern and
focus assertions, sensitive-data handling, popup/reparenting rules, and manual
assistive-technology scripts. Native/XAML ancestry, required patterns, and focus
were measured on x64 in a consuming framework. The portable skill has no bundled
UIA scenario; ARM64, High Contrast, Narrator, text-scale, magnifier, and popup
fragment passes remain pending.

**Gap:** No bundled portable process captures bounded native/XAML UI Automation
ancestry and behavior or retains the required manual assistive-technology results.

**Content:** The native/XAML UIA fragment relationship, names, runtime IDs, patterns,
focus, HWND validation, bounded capture, and required manual High Contrast,
Narrator, text-scale, and magnifier checks.

**Acceptance (pending):** External Control/Content View captures prove ancestry,
semantics, patterns, operations, focus, popups, source replacement, and bounded
failure behavior; retained manual runs cover keyboard, Narrator, contrast themes,
text scale, magnifier, localization, and supported architectures.

### Deployment and clean-machine operations

Document packaged, external-location, and unpackaged paths; bootstrap error UX;
runtime installer chaining; VCRedist; `.winmd`; architecture matrix; servicing;
repair/uninstall; enterprise provisioning; and diagnostics when Visual Studio hides
missing deployment steps.

### [Testing and diagnostics runbook](testing-and-diagnostics-runbook.md)

**Status:** Guide written with controller/child/worker topology, a bounded versioned
JSONL protocol, lifecycle vocabulary, phase deadlines, HWND validation,
process-tree cleanup, artifact schema, raw-oracle comparison, scenario catalog,
stage-based failure triage, tool correlation, WinDbg symbols/source/breakpoints,
and security limits. These patterns were measured on x64 in a consuming framework.
The portable skill bundles only the minimal-host build gate, not the described
integration controller or retained scenario artifacts.

**Gap:** No portable harness asset implements the protocol/controller and forced
timeout, cleanup, capture, dump, and artifact-retention self-tests.

**Content:** The raw oracle, subprocess scenario protocol, timeout/process-tree cleanup,
structured lifecycle events, artifact retention, WinDbg breakpoints, source lookup,
UIA/screenshot capture, and a failure-signature decision table.

**Acceptance (pending):** A bundled or consuming harness proves success, assertion
failure, protocol failure, capture failure, timeout, process-tree termination,
stream drain, cleanup failure, and retained artifacts for every applicable
scenario category and supported architecture.

## Priority 2: advanced interop

### [Mixed OLE and XAML drag/drop](mixed-ole-and-xaml-drag-drop.md)

**Status:** Guide written with layer-selection, ownership, registration,
reentrancy, editable-text, feedback, lifecycle, and validation guidance. Native
OLE text drops into a WinUI target and source rebinding after reparenting were
measured in a consuming framework. The portable skill has no bundled
mixed-transfer harness; the complete direction/device matrix remains pending.

**Gap:** The guide exists, but no bundled portable harness validates every native
and WinUI source/target direction, effect, device, reparenting, and teardown path.

**Content:** XAML's routed drag layer versus `DragDropManager`, system/OLE
interoperability, nested-loop behavior, site-bridge target registration, data-object
ownership, source UI limitations, editable-text move transactions, caret rendering,
and reparent/shutdown cleanup. Keep unvalidated branches diagnostic; add a bundled,
prescriptive implementation recipe only after behavior is stable across native and
WinUI sources and targets.

**Acceptance (pending):** Real-window automation and manual checks cover native-to-XAML,
XAML-to-native, same-island, cross-island, Copy, Move, cancellation, reparenting,
active-drag teardown, DPI, and bounded malformed data.

### [Island-scoped pointer and cursor behavior](island-pointer-and-cursor.md)

**Status:** Guide written with API-layer selection, source identity, documented
event sequences, class-handled routing, capture, cursor ownership, coordinates,
reparenting, diagnostics, and a device matrix. Mouse cursor and source rebinding
behavior were measured in a consuming framework; touch, pen, and cross-island
routing remain manual gates.

**Gap:** No bundled harness retains complete mouse, touch, pen, and cross-island
event traces or proves cursor restoration and stale-event rejection across source
replacement.

**Content:** `InputPointerSource`, `ContentIsland` availability, class-handled routed
events, capture transfer, pointer IDs, cursor lifetime, touch/pen differences, and
Loaded/Unloaded rebinding.

**Acceptance (pending):** Retained traces cover every documented terminal path for mouse,
touch, and pen; cursor restoration; multiple simultaneous pointers and islands;
unload/reload; reparenting; stale-event rejection; and DPI transitions.

### Windowless island migration watch

Track stable metadata and release notes for `XamlIsland`, `ChildSiteLink`, and
windowless composition. Keep this separate from the shipped
`DesktopWindowXamlSource` guide until the replacement path is stable and has a
complete HWND-host migration story.

## Authoring sequence

1. Freeze a minimal raw sample and integration harness as executable truth.
2. Write the Priority 0 documents from that sample. **Completed.**
3. Add code extraction tests so snippets compile against the declared package.
4. Run the architecture, DPI, focus, deployment, and shutdown matrices.
5. Publish repository-local docs and gather user failure reports.
6. File focused upstream documentation issues for gaps in Microsoft Learn or
   samples, linking reproducible evidence.
7. Propose upstream documentation contributions only with explicit repository and
   publishing approval.
8. Add the remaining Priority 1 and Priority 2 pages as their validation gates
   become real.
9. Review every Windows App SDK upgrade for stale screenshots, properties,
   package names, source paths, and future/stable API boundaries.

## Document template

Use this shape for each page:

```markdown
# Scenario name

## Applies to

- Windows App SDK version/channel
- .NET and Windows minimum
- Packaged/unpackaged
- Architectures tested

## Topology and ownership

## Minimal implementation

## Lifecycle and cleanup

## Input and coordinate spaces

## Failure signatures

## Validation matrix

## Sources

## Known gaps
```

## Tracking table

| Document | Priority | Owner | Evidence sample | Automated gate | Manual matrix | Upstream destination | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [End-to-end walkthrough](end-to-end-walkthrough.md) | 0 | Skill core | [Minimal host](assets/minimal-host/README.md) | x64/ARM64 Release build | Startup, normal close, interactive input and resize, clean machine | Microsoft Learn / WindowsAppSDK-Samples | Implemented; manual gates pending |
| [Topology and ownership](host-topology-and-ownership.md) | 0 | Skill core | [Minimal host](assets/minimal-host/README.md) | RID builds only | Initialization, normal close, rollback, parent destruction, reparenting | WinUI design notes / Microsoft Learn | Implemented; consumer terminal-state tests pending |
| [Message and focus routing](message-and-focus-routing.md) | 0 | Skill core | [Minimal host](assets/minimal-host/README.md) | RID builds only | Pretranslation, initial focus, full forward/reverse mixed traversal | Microsoft Learn / WindowsAppSDK-Samples | Implemented; consumer traversal gate pending |
| [DPI and coordinates](dpi-and-coordinate-spaces.md) | 0 | Skill core | [Minimal host](assets/minimal-host/README.md) | RID builds and conversion/unit contract | 100%-300% ordered monitor pairs | Microsoft Learn | Implemented; mixed-monitor matrix pending |
| [Popup, airspace, and z-order](popup-airspace-and-z-order.md) | 1 | Skill core | No bundled popup/airspace harness | Portfolio and link validation | Pixel, popup-edge, DPI, focus, accessibility matrix | Microsoft Learn / WindowsAppSDK-Samples | Guide written; visual matrix pending |
| [Accessibility](accessibility-across-islands.md) | 1 | Skill core | No bundled UIA scenario | Portfolio and link validation | Narrator, contrast, text-scale, magnifier, ARM64 | Microsoft Learn | Guide written; automation/manual matrices pending |
| [Testing and diagnostics](testing-and-diagnostics-runbook.md) | 1 | Skill core | [Minimal host](assets/minimal-host/README.md) | Portfolio, link, and minimal-host build validation | Forced failure, dump, architecture, retention policy | Repository-local runbook | Guide written; portable harness pending |
| [Mixed OLE and XAML drag/drop](mixed-ole-and-xaml-drag-drop.md) | 2 | Skill core | No bundled mixed-transfer harness | Portfolio and link validation | Direction/device/effect/lifecycle matrix | Microsoft Learn / WindowsAppSDK-Samples | Guide written; transfer matrix pending |
| [Island pointer and cursor](island-pointer-and-cursor.md) | 2 | Skill core | No bundled pointer harness | Portfolio and link validation | Mouse/touch/pen/island matrix | Microsoft Learn / WindowsAppSDK-Samples | Guide written; device matrix pending |

Do not mark a document complete because prose exists. Mark it complete when its
sample, cited package version, tests, clean-machine path, and declared manual checks
are current.
