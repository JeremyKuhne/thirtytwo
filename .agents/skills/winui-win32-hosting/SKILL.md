---
compatibility: Requires Windows, the .NET SDK, and the Windows App SDK; native interop examples assume a Win32 HWND host.
description: Build, review, or debug a Win32 desktop application that embeds WinUI 3 controls through DesktopWindowXamlSource, a ContentIsland, or XAML Islands. Use when asked to host WinUI inside an existing HWND application, set up the required Windows App SDK project and runtime, bridge message dispatch, focus, keyboard, pointer, DPI, theme, accessibility, popup, drag/drop, packaging, or lifetime behavior across Win32 and WinUI, or investigate failures at that boundary. Not for ordinary standalone WinUI application architecture or unrelated cross-platform UI frameworks.
license: MIT
metadata:
    applicability: dotnet
    binding: optional-overlay
    github-path: skills/winui-win32-hosting
    github-pinned: d301b736a609c1f3c6a9779e524c9b9d201540ae
    github-ref: d301b736a609c1f3c6a9779e524c9b9d201540ae
    github-repo: https://github.com/JeremyKuhne/agent-skills
    github-tree-sha: 32f7970e2c0ff71dfe92b95909d4796f707f2ac5
    maturity: canary
    portability: portable
    related: cswin32-com, cswin32-interop, security-review
    requires: none
    risk: local-write
name: winui-win32-hosting
---
# WinUI in Win32 hosting

If `overlay.md` exists beside this file, read it before acting; it contains
repository-specific bindings. This core remains usable without it.

Use this skill for the reverse-hosting boundary: an existing Win32 application
owns the process, thread, message loop, and top-level HWND, while WinUI 3 supplies
one or more islands of XAML content. Treat the result as Windows-only
cross-framework integration, not a cross-platform UI abstraction.

## Route the task

| Ask | First action |
| --- | --- |
| Create or configure a host project | Establish the project, package, runtime, architecture, manifest, and deployment model before writing host code. |
| Embed the first WinUI control | Implement the minimum thread, application, message-loop, source, sizing, focus, and disposal contract. |
| Build a reusable host abstraction | Make HWND, island, content, environment lease, reparenting, and teardown ownership explicit. |
| Debug input, DPI, popup, accessibility, or drag/drop behavior | Identify the coordinate space, input owner, native/XAML boundary, and lifetime stage before changing code. |
| Research undocumented behavior | Use authoritative metadata and source in a fixed order, preserve reproducible evidence, and separate facts from inference. |
| Produce missing technical documentation | Record the invariant, minimal reproduction, supported variants, failure signatures, and validation matrix rather than narrating one implementation. |

## Detailed guides

- [end-to-end-walkthrough.md](end-to-end-walkthrough.md) - build and run a
  code-only raw HWND host from project setup through deterministic shutdown.
- [host-topology-and-ownership.md](host-topology-and-ownership.md) - diagrams,
  ownership table, lifecycle terminal states, rollback, and reparenting.
- [message-and-focus-routing.md](message-and-focus-routing.md) - message-pump
  ordering and complete native/XAML Tab and focus stitching.
- [dpi-and-coordinate-spaces.md](dpi-and-coordinate-spaces.md) - physical/view
  pixel conversions, site-bridge sizing, Per-Monitor V2 transitions, and test
  matrix.
- [popup-airspace-and-z-order.md](popup-airspace-and-z-order.md) - bind popups
  to the current root, order and clip native siblings, and prove the composed
  result with screenshot pixels.
- [accessibility-across-islands.md](accessibility-across-islands.md) - validate
  native/XAML UI Automation ancestry, semantics, patterns, focus, bounded capture,
  and manual assistive-technology behavior.
- [testing-and-diagnostics-runbook.md](testing-and-diagnostics-runbook.md) - run
  isolated scenarios with a bounded protocol, phase deadlines, process cleanup,
  retained artifacts, stage triage, and debugger/source workflows.
- [mixed-ole-and-xaml-drag-drop.md](mixed-ole-and-xaml-drag-drop.md) - choose
  routed XAML, island-manager, or classic OLE ownership and preserve transfer,
  reentrancy, text-edit, and teardown correctness.
- [island-pointer-and-cursor.md](island-pointer-and-cursor.md) - bind the shared
  island pointer source, coexist with class handling, track terminal event paths,
  and restore cursor ownership across unload and reparenting.
- [project-setup.md](project-setup.md) - project shape, package roles, manifest,
  runtime bootstrap, architecture, and deployment.
- [host-lifecycle.md](host-lifecycle.md) - process/thread ownership,
  `Application`, XAML manager, source/site-bridge topology, reparenting, and
  teardown.
- [input-rendering-interop.md](input-rendering-interop.md) - message and focus
  routing, pointer input, DPI, theme, popup airspace, accessibility, and mixed
  drag/drop.
- [investigation-and-validation.md](investigation-and-validation.md) - evidence
  ladder, raw oracle, diagnostics, failure signatures, subprocess harness, and
  validation matrices.
- [sources.md](sources.md) - packages, Microsoft Learn pages, samples, WinUI
  design notes, and implementation source map.
- [documentation-roadmap.md](documentation-roadmap.md) - prioritized plan and
  acceptance gates for the missing end-to-end technical documentation.

## Trigger boundaries

Invoke for requests such as "host a WinUI ColorPicker in my existing Win32
window", "set up DesktopWindowXamlSource in a custom HWND message loop", or
"debug XAML island focus, DPI, airspace, packaging, or shutdown".

Do not invoke merely to build a normal WinUI top-level application, design XAML
pages, migrate UWP/WPF application architecture, or host a native HWND inside a
WinUI application. Those are adjacent but differently owned workflows.

## Non-negotiable host contract

1. Run the XAML-owning thread as STA and keep all XAML objects thread-affine.
2. Create or acquire a `DispatcherQueueController` on that thread before creating
   the WinUI `Application` or XAML content.
3. Keep exactly one compatible WinUI `Application` alive for the process and
   detect metadata or resource-provider collisions explicitly.
4. Pretranslate Win32 messages through the Windows App SDK content-input bridge
   before `TranslateMessage` and `DispatchMessage`.
5. Create `DesktopWindowXamlSource`, initialize it with the owner `WindowId`, set
   its XAML content, and treat its site bridge and content island as separate
   lifetime and coordinate-space objects.
6. Resize the site bridge when the native host changes size; convert physical
   screen pixels and XAML effective pixels using the active rasterization scale.
7. Bridge focus and Tab navigation in both directions. Do not assume native focus
   on the host HWND implies XAML focus inside the island.
8. Dispose hosted content and the XAML source before destroying their environment
   or shutting down the dispatcher queue. Make parent destruction, reparenting,
   partial construction, and dispatcher shutdown idempotent cleanup paths.
9. Validate behavior out of process with real HWNDs. Unit tests alone cannot prove
   message routing, focus, DPI, popup, UI Automation, or drag/drop integration.

## Investigation discipline

Prefer evidence in this order:

1. Installed SDK metadata and generated projections for the exact package version.
2. Official Microsoft Learn contracts and Windows App SDK samples.
3. `microsoft/microsoft-ui-xaml` and `microsoft/WindowsAppSDK` source at an
   immutable commit.
4. Minimal raw-HWND reproduction that removes application abstractions.
5. Runtime traces, window inspection, UI Automation snapshots, screenshots, and
   package-binary inspection.

Do not infer support from a Microsoft application merely using WinUI. Its shell
may host a native editor or another private component. Do not infer coordinate
spaces, ownership, or registration exclusivity from a type name; trace the
owning source path or measure it.

## Completion

A hosting change is complete only when the applicable project builds for every
supported architecture and an integration process proves startup, input, resize,
focus, disposal, and clean shutdown. Expand that proof for any touched boundary:
DPI transitions, themes, popup airspace, UI Automation, reparenting, packaging,
or OLE/XAML data transfer. Report the Windows App SDK version, deployment model,
architectures tested, manual-only checks, and unresolved documentation gaps.
