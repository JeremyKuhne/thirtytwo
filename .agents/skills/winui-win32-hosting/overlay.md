---
core: winui-win32-hosting
core-pin: b9c28f1a214370fa4aa396afc19566cf8bd67296
---

# WinUI Win32 hosting overlay

Repository-specific bindings for thirtytwo.

## Surface

- The optional hosting package is
  [src/thirtytwo.winui/thirtytwo.winui.csproj](../../../src/thirtytwo.winui/thirtytwo.winui.csproj),
  built on the native foundation in
  [src/thirtytwo/thirtytwo.csproj](../../../src/thirtytwo/thirtytwo.csproj).
- The repository targets .NET 10 and Windows 10 version 1809 or later. The
  Windows App SDK version is centrally pinned in
  [Directory.Packages.props](../../../Directory.Packages.props).
- [XamlHostEnvironment.cs](../../../src/thirtytwo.winui/XamlHostEnvironment.cs)
  owns process/thread XAML environment leases, while
  [XamlHostControl.cs](../../../src/thirtytwo.winui/XamlHostControl.cs) owns one
  `DesktopWindowXamlSource`, its content assignment, native host HWND, focus
  boundary, reparenting, and deterministic cleanup.
- WinUI control wrappers under
  [src/thirtytwo.winui](../../../src/thirtytwo.winui) reuse that host and must not
  create a second process `Application` or independently shut down a borrowed
  dispatcher queue.

## Evidence

- Use the raw-HWND oracle in
  [ControlHost](../../../src/samples/WinUI/ControlHost/ControlHost.csproj) before
  attributing a failure to the thirtytwo abstraction.
- The bundled assets opt out of the repository's Central Package Management and
  build-target imports, so their documented in-place x64 and ARM64 builds are
  supported. Use `ControlHost` as the product-local raw oracle when comparing
  behavior with thirtytwo wrappers.
- Product integration scenarios run in
  [IntegrationHost](../../../src/thirtytwo.winui_tests/IntegrationHost/IntegrationHost.csproj).
  The controller, bounded protocol, screenshot capture, UI Automation capture,
  HWND validation, and retained results live under
  [IntegrationHarness](../../../src/thirtytwo_tests/WinUI/IntegrationHarness).
- The `host-native-text-drag` scenario compiles a real `TextDragSource` / OLE
  Move into both WinUI text wrappers, with separate target-commit and
  source-deletion assertions. Its first authorized x64 run timed out after
  source readiness. Its second reached native button-down, threshold movement,
  and routed TextBox `DragEnter`, then the harness aborted before release on an
  order-dependent acceptance observation. A third authorized run sampled after
  dispatch and recorded `ContainsText=True`, Copy and Move allowed, the left
  button held, and `Accepted=None`. The empty-TextBox regression then passed. A
  fourth native run recorded the same tuple with `Accepted=Move`, then timed out
  before any observable Drop, target commit, or source deletion. A fifth run
  proved that mouse-up injection returned and routed TextBox `Drop` ran with
  `Allowed=Copy, Move; Modifiers=None; Accepted=Move`; it then timed out before
  target commit or source deletion. A sixth run recorded product Drop entry,
  deferral acquisition, `GetTextAsync` start, and native `QueryGetData` and
  `GetData` entry and return on the main STA. `GetTextAsync` did not complete;
  neither target mutation, deferral completion, OLE Move return, nor source
  deletion followed. That artifact did not record the `GetData` HRESULT. The
  temporary product observers used for that run were removed; a test-owned
  `IDataObject` adapter confirms success only in the focused unit path. No mixed
  transfer is established. Each run moves the system cursor and injects mouse
  input and therefore requires fresh, single-use explicit permission.
- ARM64 compilation currently succeeds with `CS8012` processor-mismatch
  warnings because both .NET 10 assets in `KlutzyNinja.Madowaku` 0.5.0 identify
  as AMD64. Treat this as compile-only evidence; native ARM64 execution is not
  established.
- Keep raw and wrapped scenarios aligned when changing startup, message routing,
  focus, DPI, popup/airspace, accessibility, reparenting, drag/drop, or shutdown.
  Report manual-only ARM64, mixed-monitor, Narrator, High Contrast, text-scale,
  magnifier, touch, and pen checks explicitly.

## Routing

- Use this skill for `XamlHostEnvironment`, `XamlHostControl`, WinUI wrappers,
  `ControlHost`, `IntegrationHost`, and integration-harness changes at the
  native/WinUI boundary.
- Use [cswin32-interop](../cswin32-interop/SKILL.md) for generated Win32
  declarations and raw native signatures, and
  [cswin32-com](../cswin32-com/SKILL.md) for COM vtables, CCWs, IIDs, and reference
  ownership used by the host.
- Run [security-review](../security-review/SKILL.md) for unsafe callbacks, custom
  COM providers, UI Automation capture, external drag data, protocol parsing,
  screenshots, or dumps.
- Ordinary native controls with no WinUI island remain outside this skill.

## Validation

```pwsh
dotnet build src/thirtytwo.winui/thirtytwo.winui.csproj --configuration Release
dotnet test src/thirtytwo_tests/thirtytwo_tests.csproj --configuration Release --report-trx
```

## Pending upstream sync

- The vendored core includes the XAML text drag-source guide, canonical handle
  sample, and direct-editor diagnostic from the local `agent-skills` working
  tree after pin `b9c28f1a214370fa4aa396afc19566cf8bd67296`.
- Keep this recorded divergence until those commons changes are committed. Then
  vendor the committed tree, update `core-pin` and the provenance metadata, and
  remove this section.

When the core is re-pinned, update `core-pin`, review these bindings against the
new guide/routing surface, and run the repository's strict skill validator and
relative-link checks.