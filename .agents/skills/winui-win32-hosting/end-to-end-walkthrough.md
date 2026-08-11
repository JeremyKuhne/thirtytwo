# End-to-end .NET Win32 host walkthrough

This walkthrough builds a code-only .NET executable whose raw Win32 HWND hosts a
WinUI 3 island. The complete sample is bundled in
[assets/minimal-host](assets/minimal-host/README.md); the snippets below explain
why every part exists.

## Applies to

- Windows App SDK 2.3.1, stable channel.
- .NET 10 and Windows 10 version 1809 or later.
- Framework-dependent, unpackaged executable using bootstrap auto-initialization.
- x64: built and run on Windows.
- ARM64: cross-compiled on Windows; not executed on ARM64 hardware in this review.
- x86, MSIX, clean-machine installation, and XAML markup compilation: not covered
  by the bundled sample.

`DesktopWindowXamlSource` is public in Windows App SDK 1.4 and later. Re-check
metadata, deployment properties, and source behavior before adapting this sample
to another supported release line.

## What the sample owns

The executable owns the native UI thread, its message loop, a
`DispatcherQueueController`, the process XAML `Application`, a
`WindowsXamlManager`, a top-level HWND, and one `DesktopWindowXamlSource`. The
source creates the site bridge and content island used to display a `TextBlock`,
`TextBox`, and `Button`.

The sample is intentionally raw. Establish this oracle before hiding startup and
teardown behind WPF, Windows Forms, MFC, or a custom framework abstraction.

## 1. Configure the executable project

See [MinimalWinUIHost.csproj](assets/minimal-host/MinimalWinUIHost.csproj).
The significant properties are:

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
  <RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>
  <WindowsPackageType>None</WindowsPackageType>
  <ApplicationManifest>app.manifest</ApplicationManifest>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.WindowsAppSDK" Version="2.3.1" />
  <PackageReference Include="Microsoft.Windows.CsWin32"
                    Version="0.3.298"
                    PrivateAssets="all" />
</ItemGroup>
```

`WindowsPackageType=None` enables the Windows App SDK bootstrap auto-initializer
for this unpackaged executable. The class library containing a reusable host must
not assume it owns bootstrap; the final executable does.

The Windows App SDK metapackage is the direct supported dependency. CsWin32 is
optional in a product, but this sample uses it to generate the Win32 declarations
listed in [NativeMethods.txt](assets/minimal-host/NativeMethods.txt). Do not turn
the Windows App SDK transitive package graph into a list of direct references.

## 2. Declare modern process behavior

The side-by-side [application manifest](assets/minimal-host/app.manifest) declares:

- a tested Windows version and the Windows 10/11 compatibility GUID;
- Per-Monitor V2 DPI awareness;
- long-path awareness.

Without an appropriate compatibility declaration, Windows can give Windows App
SDK components an older process behavior context. Choose `maxversiontested` for
the operating systems the application actually validates.

For unpackaged deployment, also install the matching Windows App Runtime and
Visual C++ Redistributable. Deploy the `.winmd` files selected by the package
build. A Visual Studio launch on a development machine is not a clean deployment
test.

## 3. Start on an STA thread

The entry point verifies STA and creates the dispatcher queue before any XAML
objects:

```csharp
[STAThread]
private static int Main()
{
    if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
    {
        throw new InvalidOperationException("WinUI hosting requires an STA thread.");
    }

    DispatcherQueueController dispatcher =
        DispatcherQueueController.CreateOnCurrentThread();
    try
    {
        using XamlApplication application = new();
        int result = RunMessageLoop();
        GC.KeepAlive(application);
        return result;
    }
    finally
    {
        dispatcher.ShutdownQueue();
    }
}
```

The component that owns the message loop owns the queue controller. A reusable
host should first call `DispatcherQueue.GetForCurrentThread`: borrow an existing
queue or create and later shut down its own controller. Never shut down a queue
borrowed from another framework.

## 4. Create the process XAML application

See [XamlApplication.cs](assets/minimal-host/XamlApplication.cs). One compatible
`Microsoft.UI.Xaml.Application` is shared by the process. The sample application:

1. Calls `WindowsXamlManager.InitializeForCurrentThread`.
2. Implements `IXamlMetadataProvider` by delegating to
   `XamlControlsXamlMetaDataProvider`.
3. Merges `XamlControlsResources` before standard controls are created.
4. Disposes its XAML manager after all sources are gone and before dispatcher
   shutdown.

A product that loads custom XAML controls must also compose their generated or
hand-written metadata providers and resource dictionaries. Register them before
creating dependent content and make collision precedence explicit.

## 5. Create a raw host HWND

[Program.cs](assets/minimal-host/Program.cs) registers a `WNDCLASSEXW` whose
callback is an `[UnmanagedCallersOnly]` function pointer, then creates an ordinary
overlapped window. Keep a delegate alive instead if the framework uses a managed
delegate thunk.

The HWND must exist before it can be converted into a `WindowId` for
`DesktopWindowXamlSource.Initialize`.

## 6. Attach the XAML source

The source initialization is transactional:

```csharp
DesktopWindowXamlSource source = new();
try
{
    source.Initialize(Win32Interop.GetWindowIdFromWindow((nint)window.Value));
    source.ShouldConstrainPopupsToWorkArea = true;
    source.Content = CreateContent();
    ResizeSiteBridge(source, window);
    s_xamlSource = source;
}
catch
{
    source.Content = null;
    source.Dispose();
    throw;
}
```

`Initialize` creates the `DesktopChildSiteBridge` and its child HWND. Retain the
`DesktopWindowXamlSource`; losing the source loses the island. Assign content
only after process XAML metadata and resources are ready.

The sample's `StackPanel` is code-only. Set `UseWinUI` and use the supported XAML
compiler when the project contains `.xaml` markup.

## 7. Size the site bridge

The source does not infer the desired rectangle from the parent HWND. On initial
attachment and every `WM_SIZE`, pass an integer parent-client rectangle:

```csharp
source.SiteBridge?.MoveAndResize(new RectInt32(0, 0, width, height));
```

Those dimensions are native pixels in the site bridge's parent coordinate space.
XAML element sizes are view pixels. Do not multiply XAML layout dimensions by DPI
before assigning them. See [dpi-and-coordinate-spaces.md](dpi-and-coordinate-spaces.md).

## 8. Pretranslate every retrieved message

The old native `IDesktopWindowXamlSourceNative.PreTranslateMessage` path is not
the Windows App SDK contract. A custom pump calls the exported
`ContentPreTranslateMessage` before normal translation and dispatch:

```csharp
while (true)
{
    BOOL result = PInvoke.GetMessage(out MSG message, HWND.Null, 0, 0);
    if ((int)result == -1)
    {
        throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    if (!result)
    {
        return (int)message.wParam.Value;
    }

    if (WindowsAppSdkInterop.ContentPreTranslateMessage(&message) != 0)
    {
        continue;
    }

    _ = PInvoke.TranslateMessage(message);
    _ = PInvoke.DispatchMessage(message);
}
```

The explicit P/Invoke is in
[WindowsAppSdkInterop.cs](assets/minimal-host/WindowsAppSdkInterop.cs). It uses a
blittable pointer to CsWin32's `MSG`. Do not call it after framework/platform
shutdown.

## 9. Enter XAML focus

When the host HWND receives native focus, the sample requests the first XAML focus
candidate:

```csharp
_ = source.NavigateFocus(
    new XamlSourceFocusNavigationRequest(XamlSourceFocusNavigationReason.First));
```

That is enough for one isolated island, but a mixed native/XAML dialog must also
pass Tab direction, handle `TakeFocusRequested`, continue native traversal, and
avoid focus loops. Implement the complete algorithm in
[message-and-focus-routing.md](message-and-focus-routing.md).

## 10. Tear down in ownership order

On `WM_DESTROY` and in the outer `finally` block, the sample:

1. Clears `DesktopWindowXamlSource.Content`.
2. Disposes the source.
3. Leaves the message loop.
4. Disposes `WindowsXamlManager` through the custom application.
5. Calls `DispatcherQueueController.ShutdownQueue`.

Every cleanup method is idempotent because native destruction, managed failure,
and dispatcher shutdown can converge. Do not rely on finalizers for thread-affine
XAML cleanup.

## Build and run

From the sample directory:

```pwsh
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-x64
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-arm64
dotnet run --project MinimalWinUIHost.csproj --configuration Release --runtime win-x64
```

Use the running window to inspect typing, pointer input, resizing, initial focus,
and normal shutdown.

## Failure signatures

| Symptom | First discriminating check |
| --- | --- |
| Activation or class-not-registered failure before HWND creation | Confirm runtime installation, `WindowsPackageType`, bootstrap order, and RID. |
| `RPC_E_WRONG_THREAD` during control creation | Confirm STA, current dispatcher queue, and that XAML has not shut down. |
| Control appears blank | Confirm the source is retained, content is non-null, and site-bridge bounds are nonzero. |
| Control is unthemed or fails resource lookup | Confirm `XamlControlsResources` and metadata provider registration happened first. |
| Typing or accelerators do not work | Confirm `ContentPreTranslateMessage` runs before `TranslateMessage`. |
| Tab enters but cannot leave | Implement `TakeFocusRequested` and native sibling traversal. |
| Exit hangs or later crashes | Confirm every source is disposed before queue shutdown and no producer posts new work. |
| Works only from Visual Studio | Publish and test the actual unpackaged payload with runtime, VCRedist, and `.winmd` deployment. |

## Validation matrix

| Check | Result for bundled sample |
| --- | --- |
| Release x64 build | Passed on Windows. |
| Release ARM64 build | Passed by cross-compilation on Windows. |
| x64 startup and clean shutdown | Manual check required. |
| Interactive keyboard, pointer, resize, and focus | Manual check required. |
| ARM64 execution | Not run; ARM64 hardware or emulation required. |
| Clean-machine unpackaged installation | Not run in this environment. |
| MSIX and packaged-with-external-location | Outside this sample's scope. |

## Sources

Start with the hosting API, deployment, dispatcher, and design-note entries in
[sources.md](sources.md). The bundled sample is derived from those public
contracts and verified against WinUI source behavior, not from a private
application implementation.

## Known gaps

The sample intentionally omits native sibling controls, full Tab traversal,
explicit `WM_DPICHANGED` top-level rectangle handling, mixed-monitor diagnostics,
custom XAML libraries, UI Automation assertions, popups beyond work-area policy,
and installer automation. The other Priority 0 guides cover focus, topology, and
coordinate rules; later roadmap documents own the production-hardening scenarios.
