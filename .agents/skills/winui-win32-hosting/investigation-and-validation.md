# Investigation and validation

Use this page when documentation is incomplete, behavior differs by machine, or a
host abstraction fails without a useful managed exception.

Use [testing-and-diagnostics-runbook.md](testing-and-diagnostics-runbook.md) when
building or operating the subprocess protocol, timeout/cleanup controller,
artifact capture, stage triage, or WinDbg workflow described here.

## Evidence ladder

Work from the narrowest authoritative source outward:

1. Confirm the exact .NET SDK, TFM, Windows App SDK package, architecture, deployment
   model, OS build, process identity, and apartment.
2. Inspect the installed package's `.winmd`, generated projection, build targets,
   `.deps.json`, runtime assets, and native exports.
3. Read Microsoft Learn API contracts and deployment guidance for that release.
4. Read WinUI design notes and source at an immutable commit.
5. Compare against the `WindowsAppSDK-Samples` Islands sample.
6. Build a raw-HWND oracle with no product wrapper.
7. Add stage tracing, window/UIA inspection, screenshots, and controlled input.
8. Inspect Microsoft application binaries only as corroborating evidence, never as
   an API contract.

State whether each conclusion is documented fact, source fact, measured behavior,
or inference.

## Package and metadata inspection

Useful commands include:

```pwsh
dotnet --info
dotnet list path/to/Host.csproj package --include-transitive
dotnet build path/to/Host.csproj -r win-x64
```

Inspect the selected project's `project.assets.json`, output `.deps.json`, and
published directory when a package appears present at compile time but activation
fails at runtime. Compare x86, x64, and ARM64 asset selection.

For WinRT and PE evidence:

- Use a metadata browser or `ildasm` on `.winmd` files to discover runtime classes,
  interfaces, and projected namespaces.
- Use `dumpbin /imports`, `dumpbin /exports`, or equivalent PE tooling to verify
  native DLL boundaries.
- Search both ASCII and UTF-16 strings only to identify leads. A string is evidence
  that a binary mentions a symbol, not proof that a public contract exists.
- Inspect package manifests and signatures when identifying the architecture of a
  Microsoft application or runtime package.

Do not load a product DLL into a persistent PowerShell process when later builds
must overwrite it; that process can retain a file lock.

## Source investigation map

Clone or fetch exact commits, then use `git grep` before broad web search. Important
WinUI areas include:

- `docs/design-notes/xaml-islands/` for topology, dispatcher, focus, and future
  windowless design.
- `DesktopWindowXamlSource_partial.cpp` for source initialization and HWND bridge.
- `XamlIslandRoot.cpp` for content-island input, focus, drag/drop, and lifecycle.
- `StartDragAsyncOperation.cpp` and `DropOperationTarget.cpp` for XAML drag adapters.
- `TextBoxBase.cpp`, `TextServicesHost.cpp`, and `RichEditOleCallback.cpp` for text
  host behavior.
- `focusmgr.cpp` and focus adapters for native/XAML focus changes.

Important Windows App SDK areas include bootstrap, dynamic dependencies,
deployment specifications, content/site bridge APIs, input, dispatcher queues, and
installer tests. Implementation may be split between the Windows App SDK and WinUI
repositories; absence from one is not proof the implementation is closed.

Read design notes critically. A reviewed API spec can describe shipped behavior,
while a windowless-island note can describe an intended replacement that is still
experimental. Verify the selected stable package metadata before recommending an
API.

## Raw oracle

Before debugging a framework wrapper, create a minimal executable that has only:

- STA entry point;
- runtime bootstrap;
- current-thread dispatcher queue;
- custom XAML `Application` and `WindowsXamlManager`;
- one Win32 window class and message loop;
- `ContentPreTranslateMessage`;
- one `DesktopWindowXamlSource` with one standard control;
- size, focus, close, and deterministic disposal handling.

If the oracle fails, the defect is project/runtime/platform setup. If it succeeds,
diff wrapper behavior against the oracle one boundary at a time.

## Stage-oriented diagnostics

Emit low-volume structured events at ownership transitions, not every input sample:

- process and bootstrap start;
- queue created or borrowed;
- application created or adopted;
- XAML manager initialized;
- metadata and resources registered;
- source initialized and site bridge available;
- content loaded/unloaded;
- focus request and correlation result;
- DPI and bridge bounds change;
- source detached/replaced;
- application/framework/platform shutdown stages;
- native callback failure with operation, HRESULT, and exception type.

For gesture bugs, log press eligibility, capture transfer, first routed move,
threshold, transport start, target enter/over/drop/leave, and completion. A passing
transport after a gesture rewrite does not prove the earlier transport API was
broken.

## Common failure signatures

| Symptom | Check first |
| --- | --- |
| Activation or class-not-registered error before first WinUI object | Runtime installation, bootstrap order, architecture, and package graph. |
| `RPC_E_WRONG_THREAD` | STA, owning thread, current dispatcher queue, or XAML already shut down. |
| XAML type not found | Missing/generated metadata provider or registration after content creation. |
| Standard controls render without expected theme | Missing `XamlControlsResources` or wrong dictionary order. |
| Blank island | Source lifetime, assigned content, site-bridge bounds, host visibility, and z-order. |
| Keyboard input ignored | Missing or late `ContentPreTranslateMessage`. |
| Tab gets trapped or skips island | Direction not passed to `NavigateFocus`, missing `TakeFocusRequested`, or native traversal mismatch. |
| Pointer/caret offset changes by monitor | Physical/effective pixel mix, stale rasterization scale, or wrong HWND origin. |
| Popup throws about `XamlRoot` | Detached popup/flyout without an island association. |
| Native control cannot overlay XAML | HWND airspace/z-order issue, not XAML element z-index. |
| Shutdown hangs or crashes later | Sources still alive, queue not shut down by owner, nested loop, or callbacks after framework shutdown. |
| Works from Visual Studio but not installed | Runtime/bootstrap, VCRedist, `.winmd`, package identity, or installer architecture gap. |
| Drag reaches island but no routed XAML event | A second low-level `TargetRequested` handler may have replaced XAML's target. |

## Integration harness

The complete controller, protocol, artifact, cleanup, and debugger recipe is in
[testing-and-diagnostics-runbook.md](testing-and-diagnostics-runbook.md).

Run each lifecycle-sensitive scenario in a fresh subprocess. XAML process state,
`Application.Current`, dispatcher shutdown, and native registrations make repeated
in-process tests poor isolation.

A robust harness should:

- launch a purpose-built scenario executable with a bounded timeout;
- emit a versioned, machine-readable event protocol;
- report a top-level HWND and native thread ID;
- validate that HWND belongs to the child process before messaging or capture;
- bound stdout, stderr, event count, UIA nodes, screenshots, and cleanup time;
- kill the process tree on timeout and still drain output tasks;
- retain result JSON, logs, screenshots, UIA snapshots, OS/runtime/package details,
  and the last completed lifecycle stage.

Use real windows for focus, message, DPI, UIA, airspace, and drag/drop behavior.
Direct callback tests are useful for malformed data and ownership branches, but do
not prove OS registration and routing.

## Validation matrix

Scale proof to the touched boundary:

| Area | Minimum evidence |
| --- | --- |
| Startup/lifetime | Owned and borrowed queue, compatible/incompatible application, MTA and wrong-thread rejection, normal close, parent destruction, dispatcher shutdown, final process exit. |
| Host control | Empty/content factory construction, replacement, multiple hosts, stress, reparent success and rollback, parent destruction. |
| Input/focus | Typing, accelerators, Tab and Shift+Tab across both boundaries, pointer, capture loss, IME where applicable. |
| DPI/layout | Resize, multiple scales, both monitor directions, negative coordinates, maximize/restore, popup position. |
| Airspace | Native above/below XAML, clipping, no focus theft, screenshot pixel assertions. |
| Accessibility | Native/XAML ancestry, runtime IDs, names, patterns, focus, High Contrast and Narrator manual pass. |
| Deployment | Debug and Release publish for each RID, clean machine, runtime missing/repair path, packaged/unpackaged path actually shipped. |
| Drag/drop | Same and cross-control copy/move, Ctrl, cancel, self-range rejection, native/WinUI targets, source selection, target caret, reparent and shutdown cleanup. |

## Investigation report

Record:

- exact versions, commit pins, architecture, OS, deployment model, and repro steps;
- the first stage that differs from the raw oracle;
- facts, measurements, and remaining inferences separately;
- commands and artifacts that another engineer can reproduce;
- behavior not tested, especially ARM64, High Contrast, Narrator, mixed DPI, and
  clean-machine deployment;
- whether the finding belongs in product code, a reusable skill, local overlay,
  public documentation, or an upstream issue.
