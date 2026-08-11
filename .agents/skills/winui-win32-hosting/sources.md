# Packages, documentation, and source map

Use this catalog to start research. Always confirm the selected stable Windows App
SDK package because API availability and deployment behavior change by release.

The bundled Priority 0 guides and sample were verified against Windows App SDK
2.3.1 and WinUI source commit
[`29ebf098f70df518b57b754130bc94004be8c6bc`](https://github.com/microsoft/microsoft-ui-xaml/tree/29ebf098f70df518b57b754130bc94004be8c6bc).
Moving-branch links below are discovery entry points; pin the exact commit again
when updating an implementation claim.

## Packages

| Resource | Use |
| --- | --- |
| [`Microsoft.WindowsAppSDK` on NuGet](https://www.nuget.org/packages/Microsoft.WindowsAppSDK/) | Supported metapackage, versions, dependencies, and project links. |
| [Windows App SDK release channels](https://learn.microsoft.com/windows/apps/windows-app-sdk/release-channels) | Current/maintenance support and stable, preview, experimental boundaries. |
| [`Microsoft.Windows.CsWin32`](https://www.nuget.org/packages/Microsoft.Windows.CsWin32/) | Optional Win32 projection source generator. |
| [CsWin32 source](https://github.com/microsoft/CsWin32) | Generator configuration, metadata behavior, and issues. |

Reference the Windows App SDK metapackage directly. Treat component packages and
build-tool packages in its transitive graph as implementation detail unless the
application directly consumes one under a documented contract.

## Setup and deployment documentation

- [Deployment architecture](https://learn.microsoft.com/windows/apps/windows-app-sdk/deployment-architecture)
- [Use the runtime from unpackaged apps](https://learn.microsoft.com/windows/apps/windows-app-sdk/use-windows-app-sdk-run-time)
- [Deploy framework-dependent unpackaged apps](https://learn.microsoft.com/windows/apps/windows-app-sdk/deploy-unpackaged-apps)
- [Windows App SDK downloads](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)
- [Bootstrapper API](https://learn.microsoft.com/windows/apps/windows-app-sdk/api-reference/cs-bootstrapper-apis/)
- [Project properties and auto-initializers](https://learn.microsoft.com/windows/apps/package-and-deploy/project-properties)
- [Application manifests](https://learn.microsoft.com/windows/win32/sbscs/application-manifests)
- [Per-Monitor V2 DPI awareness](https://learn.microsoft.com/windows/win32/hidpi/dpi-awareness-context)

## Hosting APIs

- [`DesktopWindowXamlSource`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.desktopwindowxamlsource)
- [`DesktopWindowXamlSource.Initialize`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.desktopwindowxamlsource.initialize)
- [`DesktopWindowXamlSource.NavigateFocus`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.desktopwindowxamlsource.navigatefocus)
- [`DesktopWindowXamlSource.ShouldConstrainPopupsToWorkArea`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.desktopwindowxamlsource.shouldconstrainpopupstoworkarea)
- [`WindowsXamlManager.InitializeForCurrentThread`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.windowsxamlmanager.initializeforcurrentthread)
- [`DispatcherQueueController.CreateOnCurrentThread`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.dispatching.dispatcherqueuecontroller.createoncurrentthread)
- [`DesktopChildSiteBridge`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.content.desktopchildsitebridge)
- [`DesktopSiteBridge.WindowId`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.content.desktopsitebridge.windowid)
- [`XamlRoot`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.xamlroot)
- [`UIElement.XamlRoot`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.xamlroot)
- [`XamlRoot.ContentIsland`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.xamlroot.contentisland)
- [`InputPointerSource`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.inputpointersource)
- [`InputPointerSource.GetForIsland`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.inputpointersource.getforisland)
- [`InputPointerSource.Cursor`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.inputpointersource.cursor)
- [`InputSystemCursor`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.inputsystemcursor)
- [`ContentIsland`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.content.contentisland)
- [`Win32Interop`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.win32interop)

API reference pages describe members but do not form a complete host recipe. Pair
them with the design notes and sample below.

## Popup, airspace, and native windowing

- [`FlyoutBase.ShouldConstrainToRootBounds`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.primitives.flyoutbase.shouldconstraintorootbounds)
- [`SetWindowPos`](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowpos)
- [Child windows](https://learn.microsoft.com/windows/win32/winmsg/window-features#child-windows)
- [Window styles](https://learn.microsoft.com/windows/win32/winmsg/window-styles)
- [`GetWindowThreadProcessId`](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid)
- [`EnumChildWindows`](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-enumchildwindows)
- [`Graphics.CopyFromScreen`](https://learn.microsoft.com/dotnet/api/system.drawing.graphics.copyfromscreen)

## Accessibility and UI Automation

- [UI Automation tree overview](https://learn.microsoft.com/windows/win32/winauto/uiauto-treeoverview)
- [UI Automation control patterns](https://learn.microsoft.com/windows/win32/winauto/uiauto-controlpatternsoverview)
- [`AutomationProperties`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.automation.automationproperties)
- [`FrameworkElementAutomationPeer`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.automation.peers.frameworkelementautomationpeer)
- [Accessibility testing](https://learn.microsoft.com/windows/apps/design/accessibility/accessibility-testing)
- [Contrast themes](https://learn.microsoft.com/windows/apps/design/accessibility/high-contrast-themes)
- [Accessibility Insights for Windows](https://accessibilityinsights.io/docs/windows/overview/)
- [Inspect](https://learn.microsoft.com/windows/win32/winauto/inspect-objects)

## Integration testing and diagnostics

- [`Process.WaitForExitAsync`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.waitforexitasync)
- [`Process.Kill(Boolean)`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.kill)
- [Collect user-mode dumps](https://learn.microsoft.com/windows/win32/wer/collecting-user-mode-dumps)
- [WinDbg symbol path](https://learn.microsoft.com/windows-hardware/drivers/debugger/symbol-path)
- [WinDbg source path](https://learn.microsoft.com/windows-hardware/drivers/debugger/source-path)
- [WinDbg `.reload`](https://learn.microsoft.com/windows-hardware/drivers/debuggercmds/-reload--reload-module-)

## Drag/drop APIs

- [Drag and drop overview](https://learn.microsoft.com/windows/apps/develop/data/drag-and-drop)
- [`UIElement.StartDragAsync`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.startdragasync)
- [`DragEventArgs.GetPosition`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.drageventargs.getposition)
- [`DragDropManager`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.dragdrop.dragdropmanager)
- [`DragOperation`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.dragdrop.dragoperation)
- [`IDropOperationTarget`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.input.dragdrop.idropoperationtarget)
- [`DragUIOverride.Clear`](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.draguioverride.clear)
- [`DataPackage`](https://learn.microsoft.com/uwp/api/windows.applicationmodel.datatransfer.datapackage)

## Official samples

- [Windows App SDK Islands samples](https://github.com/microsoft/WindowsAppSDK-Samples/tree/main/Samples/Islands)
- [SimpleIslandApp sample page](https://learn.microsoft.com/samples/microsoft/windowsappsdk-samples/simpleislandapp/)
- [Windows App SDK installer sample](https://github.com/microsoft/WindowsAppSDK-Samples/tree/main/Samples/Installer)
- [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery) for control behavior and theme resources, not Win32 host ownership.

Keep a minimal copy of the current official island sample as an oracle. Do not let a
large application framework become the only reproduction.

## WinUI design notes

The most detailed hosting documentation lives in the WinUI source repository under
[`docs/design-notes/xaml-islands`](https://github.com/microsoft/microsoft-ui-xaml/tree/main/docs/design-notes/xaml-islands), not only on Microsoft Learn:

- [`desktopwindowxamlsource.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/desktopwindowxamlsource.md): reviewed API spec, startup/shutdown, custom `Application`, focus, and removed legacy interop.
- [`xaml-islands-and-dispatcherqueue.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/xaml-islands-and-dispatcherqueue.md): queue ownership, custom pumps, pretranslation, and organized shutdown.
- [`xaml-island-focus-navigation.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/xaml-island-focus-navigation.md): native/XAML Tab stitching and focus internals.
- [`xaml-island-impl.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/xaml-island-impl.md): thread tree, island roots, `XamlRoot`, popup association, and useful breakpoints.
- [`xaml-island-type.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/xaml-island-type.md): `DesktopWindowXamlSource` versus lower-level island composition.
- [`windowless-xaml-islands.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/windowless-xaml-islands.md): future/windowless topology and `ChildSiteLink`.
- [`xaml-islands.md`](https://github.com/microsoft/microsoft-ui-xaml/blob/main/docs/design-notes/xaml-islands/xaml-islands.md): terminology and roadmap context.

Pin the repository commit when relying on implementation or roadmap language.
Statements that an API "will eventually" replace another are design direction, not
proof that the replacement is stable in the package being used.

## WinUI implementation source

Start from [`microsoft/microsoft-ui-xaml`](https://github.com/microsoft/microsoft-ui-xaml) and inspect these paths at the package-corresponding commit where possible:

- [`DesktopWindowXamlSource_partial.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/DesktopWindowXamlSource_partial.cpp)
- [`XamlIslandRoot.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/core/core/elements/XamlIslandRoot.cpp)
- [`StartDragAsyncOperation.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/StartDragAsyncOperation.cpp)
- [`DropOperationTarget.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/DropOperationTarget.cpp)
- [`AutomaticDragHelper.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/AutomaticDragHelper.cpp)
- [`TextBoxBase.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/core/native/text/Controls/TextBoxBase.cpp)
- [`TextServicesHost.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/core/native/text/Controls/TextServicesHost.cpp)
- [`RichEditOleCallback.cpp`](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/core/native/text/Controls/RichEditOleCallback.cpp)
- focus manager and content-root adapter sources under `dxaml/xcp/components` and
  `dxaml/xcp/core`.

Useful implementation breakpoints named by the design notes include
`DesktopWindowXamlSource::InitializeImpl`, `CXamlIslandRoot::InitializeCommon`,
`CXamlIslandRoot::PreTranslateMessage`, `CXamlIslandRoot::OnIslandGotFocus`, and
`CFocusManager::UpdateFocus`.

## Windows App SDK source

Use [`microsoft/WindowsAppSDK`](https://github.com/microsoft/WindowsAppSDK) for:

- bootstrapper and .NET wrapper implementation;
- dynamic dependency and deployment specifications;
- runtime package layout and installer behavior;
- dispatcher, content, input, windowing, and platform API contracts that are not
  implemented in the WinUI repository;
- issues and discussions for runtime activation, packaging, and architecture
  failures.

Important starting paths include `dev/Bootstrap`, `dev/DynamicDependency`,
`specs/Deployment`, and installer tests. Some `Microsoft.UI.Input` or platform
implementation is not published; stop at the public contract rather than inventing
an internal explanation.

## Native platform documentation

Use Microsoft Win32 documentation for:

- window creation, child styles, z-order, focus, dialog navigation, message loops,
  and DPI;
- COM apartment initialization and reference counting;
- OLE `DoDragDrop`, `RegisterDragDrop`, `IDataObject`, `IDropSource`, and
  `IDropTarget`;
- `FORMATETC`, `STGMEDIUM`, global memory, and clipboard formats;
- UI Automation fragment roots, runtime IDs, control types, and patterns.

Prefer Windows SDK metadata and headers over copying declarations from samples.

For classic OLE drag/drop, start with:

- [`OleInitialize`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-oleinitialize)
- [`OleUninitialize`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-oleuninitialize)
- [`DoDragDrop`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-dodragdrop)
- [`RegisterDragDrop`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-registerdragdrop)
- [`RevokeDragDrop`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-revokedragdrop)
- [`IDataObject`](https://learn.microsoft.com/windows/win32/api/objidl/nn-objidl-idataobject)
- [`IDropSource`](https://learn.microsoft.com/windows/win32/api/oleidl/nn-oleidl-idropsource)
- [`IDropTarget`](https://learn.microsoft.com/windows/win32/api/oleidl/nn-oleidl-idroptarget)
- [`FORMATETC`](https://learn.microsoft.com/windows/win32/api/objidl/ns-objidl-formatetc)
- [`STGMEDIUM`](https://learn.microsoft.com/windows/win32/api/objidl/ns-objidl-ustgmedium-r1)
- [`ReleaseStgMedium`](https://learn.microsoft.com/windows/win32/api/ole2/nf-ole2-releasestgmedium)

## Corroborating applications

A Microsoft application using WinUI does not prove its primary control is a WinUI
control. For example, modern Notepad uses WinUI/XAML for surrounding UI but hosts a
native Microsoft 365 `RichEditD2DPT` editor. See the RichEdit team's
[Windows 11 Notepad](https://devblogs.microsoft.com/math-in-office/windows-11-notepad/)
article. Package manifests, signed binaries, imports, WinMD, and runtime window
classes can corroborate architecture, but private behavior is not a supported API.

## Freshness checklist

Before applying this catalog:

1. Record the installed package version and stable-channel support status.
2. Resolve the package graph and runtime architecture again.
3. Pin source links to a commit for any implementation claim.
4. Check whether formerly experimental island APIs are stable in current metadata.
5. Re-run the official sample and a clean-machine deployment.
6. Label unverified future direction and unavailable source explicitly.
