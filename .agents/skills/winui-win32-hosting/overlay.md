---
core: winui-win32-hosting
core-pin: d301b736a609c1f3c6a9779e524c9b9d201540ae
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
- Do not build the core's bundled `assets/minimal-host` in place. The repository's
  Central Package Management settings apply from the root and reject the sample's
  intentionally explicit package versions. Copy the sample directory outside the
  repository hierarchy for a standalone build, or use `ControlHost` as the local
  raw oracle.
- Product integration scenarios run in
  [IntegrationHost](../../../src/thirtytwo.winui_tests/IntegrationHost/IntegrationHost.csproj).
  The controller, bounded protocol, screenshot capture, UI Automation capture,
  HWND validation, and retained results live under
  [IntegrationHarness](../../../src/thirtytwo_tests/WinUI/IntegrationHarness).
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

When the core is re-pinned, update `core-pin`, review these bindings against the
new guide/routing surface, and run the repository's strict skill validator and
relative-link checks.