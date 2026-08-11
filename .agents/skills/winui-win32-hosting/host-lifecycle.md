# Host topology and lifecycle

Use this page when implementing the hosting environment, reusable child control,
reparenting, or shutdown behavior.

See [host topology and ownership](host-topology-and-ownership.md) for the object,
state, sequence, and reparenting diagrams that apply these rules.

## Applies to

- Windows App SDK 1.4 or later HWND-backed XAML islands.
- Raw Win32 and framework wrappers that own or borrow a native UI thread and
  dispatcher queue.
- One process XAML `Application`, one or more sources, and deterministic
  construction, reparenting, parent-destruction, and shutdown paths.
- Lower-level windowless islands only where explicitly identified; their ownership
  topology is not assumed to match `DesktopWindowXamlSource`.

## Ownership model

A hosted tree crosses several owners. Name them explicitly in code and tests.

| Object | Normal owner | Important boundary |
| --- | --- | --- |
| Top-level and host HWNDs | Existing Win32 framework/application | Physical pixels, native thread affinity, message loop. |
| `DispatcherQueueController` | Component that owns the UI thread's message pump | Create only when no queue exists; shut down only when owned. |
| WinUI `Application` | Process | Exactly one compatible instance; retains metadata and resources. |
| `WindowsXamlManager` | XAML-owning thread/environment | Represents thread XAML lifetime; XAML cannot be restarted in the process after complete shutdown. |
| `DesktopWindowXamlSource` | One host instance | Creates and owns a site bridge and content island; must remain strongly referenced. |
| XAML content | Application or wrapper policy | Clearing host content does not imply disposing arbitrary content. |
| Site-bridge child HWND | `DesktopChildSiteBridge` | Receives native focus, hit testing, z-order, and OLE registration. It is not the application's wrapper HWND. |

General Windows App SDK hosting can initialize XAML on more than one suitable
thread, but each thread needs its own queue and thread-bound XAML state while the
process still shares one `Application`. If the surrounding framework does not
explicitly support that model, designate one XAML UI thread and reject accidental
second-thread initialization.

## Initialization sequence

1. Enter through an STA entry point. Verify the apartment rather than relying on
   a template attribute that another host might ignore.
2. Establish the native UI dispatcher/message-loop owner.
3. Call `DispatcherQueue.GetForCurrentThread`. Borrow an existing queue or create
   `DispatcherQueueController.CreateOnCurrentThread` and remember ownership.
4. Create or adopt the one process `Microsoft.UI.Xaml.Application`. A custom
   application must expose every metadata provider and application resource
   dictionary needed by hosted controls.
5. Initialize XAML for the current thread with
   `WindowsXamlManager.InitializeForCurrentThread` unless the selected supported
   startup path has already done so.
6. Register built-in `XamlControlsXamlMetaDataProvider` and
   `XamlControlsResources`, then register library providers and dictionaries in
   deterministic order.
7. Create the native parent/host HWND.
8. Construct `DesktopWindowXamlSource`, call `Initialize` with the native
   parent's `WindowId`, subscribe to focus events, assign content, and size the
   returned site bridge.
9. Run the message loop with content pretranslation before normal Win32
   translation and dispatch.

Do not create controls that depend on metadata or theme resources before their
providers and dictionaries are registered.

## Application composition

One `Application` is allowed per process. A reusable component has two valid
modes:

- **Create:** construct a host application, retain it for the process, initialize
  XAML composition, and register built-in plus library metadata/resources.
- **Adopt:** require the existing application to implement a documented
  composition contract, verify its registries belong to the calling XAML thread,
  and register through that contract.

Do not replace an incompatible `Application.Current` or hide partially completed
application construction by creating another instance. Fail with an
initialization stage, thread IDs, process architecture, HRESULT, and inner
exception.

Metadata lookup uses first-provider precedence. Resource dictionaries use WinUI
merge precedence, where later dictionaries can override earlier keys. Make both
orders deterministic, deduplicate registrations, and surface collisions through
logs or events.

## Creating a source

A source factory should be transactional:

1. Construct `DesktopWindowXamlSource`.
2. Convert the owner HWND with `Win32Interop.GetWindowIdFromWindow`.
3. Call `Initialize` and retain the resulting site bridge.
4. Set popup policy such as `ShouldConstrainPopupsToWorkArea` deliberately.
5. Subscribe to `GotFocus` and `TakeFocusRequested`.
6. Move and resize the site bridge to current client bounds.
7. Assign content last.
8. On any failure, unsubscribe and dispose the source before rethrowing.

The host wrapper should use `WS_CHILD | WS_VISIBLE | WS_TABSTOP` and normally
`WS_CLIPCHILDREN | WS_CLIPSIBLINGS`. Give the wrapper a nonpainting or transparent
background policy appropriate to the native framework; the island paints its own
child HWND.

## Loaded state and island services

`DesktopWindowXamlSource.Initialize` creates the bridge, but element-level
services that depend on `XamlRoot` or `XamlRoot.ContentIsland` may not be available
until content enters the live tree. For pointer sources, drag/drop managers,
composition services, and island-scoped input:

1. Try to attach after assigning content.
2. If `XamlRoot` or `ContentIsland` is null, subscribe once to `Loaded`.
3. Detach island-scoped services on `Unloaded` or before source replacement.
4. Reacquire every island-scoped object after the content is attached to a new
   source; never reuse an input source or manager from the old island.

## Resize and DPI

The site bridge uses an integer rectangle in its parent HWND coordinate system.
Resize it on native size changes and again after relevant DPI transitions. Keep
logical layout in XAML; do not pre-scale XAML element dimensions and then let
XAML scale them again.

Coordinate conversion must state both origin and unit. A common conversion from
physical screen pixels into XAML effective pixels is:

1. Subtract the site-bridge screen origin.
2. Divide by `XamlRoot.RasterizationScale`.
3. Transform relative to the target element if it does not fill the island.

Do not assume the wrapper HWND, site-bridge HWND, content root, and target control
share an origin.

## Reparenting state machine

A `DesktopWindowXamlSource` is attached to the `WindowId` used during
initialization. Reparenting a wrapper HWND does not retarget the existing source.
Use a transactional replacement:

1. Validate that the new parent exists and belongs to the owner native thread.
2. Suspend registrations tied to the old site-bridge HWND.
3. Notify derived behavior that the island is changing.
4. Save content, unsubscribe source events, clear content, and dispose the old
   source.
5. Change the wrapper's native parent.
6. Create and initialize a new source for the wrapper HWND.
7. Reassign content, reacquire island services after load, and resume native
   registrations against the new site-bridge HWND.
8. If a step fails, restore the original parent and create a replacement source
   there. If recovery also fails, dispose the host and report both failures.

Never leave an OLE drop target, input source, event handler, or composition visual
bound to the old bridge.

## Shutdown sequence

Application-level `DispatcherQueue.ShutdownStarting` is the cleanup window before
framework and platform shutdown. Tear down high-level objects there or earlier:

1. Stop timers, watchers, drag operations, and work producers.
2. Unsubscribe element, source, island-input, and focus events.
3. Clear `DesktopWindowXamlSource.Content`.
4. Dispose every `DesktopWindowXamlSource` and island-scoped object.
5. Release host/environment leases.
6. Dispose the thread's `WindowsXamlManager` if explicitly owned.
7. Call `DispatcherQueueController.ShutdownQueue` only when the host created the
   controller.
8. Release bootstrap/runtime activation after every Windows App SDK object and
   thread is gone.

Disposing the last public wrapper lease should not opportunistically tear down
thread XAML while the dispatcher remains active; a later host may need it. Bind
native environment teardown to the owning dispatcher shutdown.

XAML shutdown flushes outstanding asynchronous events, including `Unloaded`.
Cleanup must therefore be reentrancy-aware and idempotent. After complete XAML
shutdown, do not attempt to restart XAML in the same process.

## Failure and rollback rules

- Construction failure must dispose every successfully acquired layer in reverse
  order while preserving the original exception.
- Parent destruction must run the same XAML cleanup as explicit disposal.
- Native window-procedure callbacks must not let managed exceptions cross the ABI.
  Log the operation and HRESULT, restore invariants, and return the documented
  native fallback.
- Finalizers are not a thread-affine cleanup strategy. Retain surviving hosts for
  owner-thread dispatcher cleanup instead.
- Keep delegates, process `Application`, and other managed objects alive for as
  long as native code can call them.
