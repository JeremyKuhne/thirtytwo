# Project and deployment setup

Use this page when creating or repairing the executable project that owns a
Win32 message loop and hosts WinUI 3 content. The application is Windows-only;
"cross-framework" here means Win32 and WinUI in one process, not portability to
other operating systems.

For a complete buildable sequence, use the
[end-to-end walkthrough](end-to-end-walkthrough.md) and its bundled
[minimal host](assets/minimal-host/README.md).

## Applies to

- Windows App SDK 1.4 or later `DesktopWindowXamlSource` hosts.
- .NET executables and libraries that participate in WinUI/Win32 integration.
- Packaged, packaged-with-external-location, and unpackaged deployment decisions.
- x86, x64, and ARM64 project planning; validate only the architectures the
  product ships.

## Choose the deployment model first

| Model | Runtime activation | Identity | Installer responsibility |
| --- | --- | --- | --- |
| MSIX packaged | Package graph activates the declared Windows App SDK framework dependency. | Present. | Deploy the app package and its framework dependency. |
| Packaged with external location | Bootstrap or deployment manager activation may still be required; follow the current deployment guide. | Present, with external payload. | Deploy identity plus the external files and runtime. |
| Unpackaged | Bootstrapper adds the matching framework package to the process package graph. | Absent unless supplied separately. | Install the Windows App Runtime and prerequisites, then deploy the application payload. |

Do not postpone this choice. It controls project properties, startup order,
available APIs, clean-machine testing, and error handling.

## Minimum code-only executable

A code-only host does not need the WinUI XAML compiler merely to instantiate
controls in C#. Pin the latest supported patch of one stable Windows App SDK
line centrally when the repository uses central package management.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.17763.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <WindowsPackageType>None</WindowsPackageType>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="X.Y.Z" />
  </ItemGroup>
</Project>
```

Adjust the .NET target to the repository baseline. Keep a Windows target
platform version in the TFM so WinRT projections and platform analyzers know the
minimum contract. Build separate `win-x86`, `win-x64`, and `win-arm64` outputs
for every supported architecture; do not assume an AnyCPU output can load an
arbitrary native Windows App Runtime.

`WindowsPackageType=None` opts an unpackaged executable into Windows App SDK
bootstrap auto-initialization. Since Windows App SDK 1.2, executable projects
receive auto-initializers by default while class libraries do not. A class
library must not silently become the process bootstrap owner. If the executable
uses explicit bootstrap control, disable the generated bootstrap initializer
and call the documented .NET `Bootstrap` API before any other Windows App SDK
API.

## XAML markup and control libraries

Set `UseWinUI` when the project compiles `.xaml` files or otherwise needs the
WinUI XAML build pipeline:

```xml
<PropertyGroup>
  <UseWinUI>true</UseWinUI>
</PropertyGroup>
```

A code-only library can omit it. Either kind of library may expose controls,
resource dictionaries, or XAML metadata. The process `Application` must compose
every required `IXamlMetadataProvider` and merge the corresponding resource
dictionaries before dependent content is created. Generated providers from
XAML compilation and hand-written providers follow the same application-level
ownership rule.

A library reference to `Microsoft.WindowsAppSDK` supplies compile-time WinRT
projections. It does not make that library responsible for bootstrap,
`DispatcherQueue`, process `Application`, or shutdown.

## Package roles

| Package or assembly | Role | Direct-reference rule |
| --- | --- | --- |
| `Microsoft.WindowsAppSDK` | Supported metapackage for WinUI, runtime activation targets, projections, resources, and build assets. | Required in the executable or a referenced integration library. Pin one supported stable patch. |
| `Microsoft.Windows.CsWin32` | Generates strongly typed Win32 P/Invoke and COM projections from Windows metadata. | Optional. Use when the host owns native APIs and the repository accepts source generation. |
| `Microsoft.WindowsAppRuntime.Bootstrap.Net.dll` | .NET wrapper for explicit bootstrap initialization and shutdown. | Supplied by Windows App SDK build assets; consume through documented APIs rather than adding arbitrary runtime DLL references. |
| `Microsoft.Windows.SDK.BuildTools` | Windows SDK metadata and build support in the resolved Windows App SDK graph. | Normally transitive. Add directly only for an independently justified build requirement. |
| `Microsoft.Web.WebView2` | WebView2 runtime projection used by WinUI's WebView2 control. | Normally transitive. Pin directly only when the application directly owns WebView2 APIs or version policy. |
| `Microsoft.WindowsAppSDK.*` component packages | Internal/componentized dependency closure of the metapackage. | Do not mirror the transitive list as direct references. Inspect it for diagnosis, not project authoring. |

Do not add an independent CsWinRT package merely because WinRT types appear in
source. The supported Windows App SDK metapackage carries the projections and
build integration needed by ordinary .NET consumers. Add projection tooling
explicitly only when authoring a Windows Runtime component requires it.

Record the resolved graph for every upgrade:

```pwsh
dotnet list path/to/Host.csproj package --include-transitive
```

The graph can change between Windows App SDK releases. Treat component package
names and versions as diagnostics, not as the public dependency contract.

## Native interop surface

A typical host needs these API families:

- Win32 window class, creation, sizing, focus, message-loop, and DPI APIs.
- `Microsoft.UI.Win32Interop.GetWindowIdFromWindow` and
  `GetWindowFromWindowId` for HWND/`WindowId` conversion.
- `Microsoft.UI.Windowing.Core.dll!ContentPreTranslateMessage` in a custom
  message loop. This export may require an explicit blittable P/Invoke when it
  is absent from the selected metadata projection.
- Optional OLE `IDataObject`, `IDropSource`, `IDropTarget`, `DoDragDrop`, and
  `RegisterDragDrop` APIs for cross-framework data transfer.

Use generated Win32 declarations when available. Preserve exact `BOOL`,
`HRESULT`, pointer, ownership, last-error, byte-count, and apartment contracts.
Do not create local duplicates of WinRT types already projected by Windows App
SDK.

## Application manifest

An unpackaged executable needs a side-by-side manifest that declares modern OS
compatibility. Without it, Windows App SDK components can receive an older
Windows behavior context. Start from the current Microsoft deployment guidance:

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <maxversiontested Id="10.0.17763.0" />
      <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
    </application>
  </compatibility>
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
      <longPathAware xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">true</longPathAware>
    </windowsSettings>
  </application>
</assembly>
```

Choose `maxversiontested` deliberately. A higher value opts into behavior for a
newer Windows release; it is not a harmless documentation field. Add Common
Controls v6 only when the surrounding Win32 UI needs it.

## Runtime and redistribution

For framework-dependent unpackaged deployment:

1. Install the matching Windows App Runtime architecture with the official
   installer or deploy all documented MSIX runtime packages.
2. Install the supported Visual C++ Redistributable.
3. Bootstrap before first Windows App SDK use and shut the bootstrapper down
   only after all Windows App SDK objects and threads have ended.
4. Deploy required `.winmd` files. Their absence can break apartment marshalling
   only on some machines, so a local success does not prove they are optional.
5. Test the actual installer on a clean machine for x86, x64, and ARM64 as
   applicable.

Do not solve a missing runtime by copying framework DLLs beside the executable.
That bypasses package graph, servicing, resources, COM/WinRT registration, and
DDLM behavior.

## Setup validation

Before implementing a reusable host abstraction, prove a raw code-only oracle:

- The process starts without Visual Studio deployment support.
- A top-level Win32 HWND appears.
- A `DesktopWindowXamlSource` displays one standard WinUI control.
- Keyboard and pointer input work through the custom message loop.
- Resize and a Per-Monitor V2 DPI transition preserve alignment.
- Closing the window disposes the island and exits without a hang or late
  `RPC_E_WRONG_THREAD` failure.
- The same published payload works on a clean machine with only documented
  prerequisites.

Only then move the sequence behind framework-specific wrappers.
