# Message and focus routing cookbook

This guide stitches a Win32 tab sequence to focusable elements inside one or more
`DesktopWindowXamlSource` islands. Calling `ContentPreTranslateMessage` is
necessary but does not by itself tell XAML whether focus entered an island through
Tab or Shift+Tab, nor where focus should go after leaving it.

## Applies to

- Windows App SDK 1.4 or later `DesktopWindowXamlSource`.
- Custom Win32 message loops and framework loops that expose a message-filter hook.
- Native controls before/after one or more XAML islands.
- Islands containing normal XAML controls and optional HWND-hosted descendants such
  as WebView2.
- Keyboard focus; directional/gamepad navigation needs an expanded direction map.

The bundled [minimal host](assets/minimal-host/README.md) demonstrates
pretranslation and initial focus. The full traversal algorithm below is the
production boundary.

## Two focus models

- Win32 tracks one keyboard-focus HWND per input queue. For an HWND-backed XAML
  island, that is normally the site-bridge child HWND.
- XAML tracks one focused element per island through that island's focus manager.

`SetFocus(siteBridgeHwnd)` changes native focus but does not communicate whether
the user arrived forward or backward. `NavigateFocus` supplies that intent and
selects the first or last focusable XAML element.

## Message-loop ordering

For every message retrieved from the native queue:

1. Call `ContentPreTranslateMessage`.
2. If it returns handled, do not call `TranslateMessage` or `DispatchMessage`.
3. If it did not handle a Tab key, run native/XAML boundary traversal.
4. If boundary traversal handles the message, stop.
5. Let the native framework perform its normal accelerator/dialog processing.
6. Translate and dispatch anything still unhandled.

```csharp
bool handledByXaml = ContentPreTranslateMessage(&message) != 0;
if (handledByXaml)
{
    continue;
}

if (TryHandleTabNavigation(topLevelWindow, message, islands))
{
    continue;
}

if (PInvoke.IsDialogMessage(topLevelWindow, &message))
{
    continue;
}

_ = PInvoke.TranslateMessage(message);
_ = PInvoke.DispatchMessage(message);
```

Frameworks differ in where dialog and accelerator processing occurs. Preserve their
ordering while ensuring content pretranslation happens once, not once per wrapper.
A message handled twice can invoke buttons or accelerators twice.

## Entering an island

When an unhandled `WM_KEYDOWN/VK_TAB` originates in the relevant top-level window:

1. Read Shift state to determine direction.
2. Ask native dialog traversal for the next/previous tab-stop HWND.
3. Map that HWND to the owning `DesktopWindowXamlSource`. The wrapper HWND and
   site-bridge HWND can differ; use the host's documented tab-stop HWND.
4. Call `NavigateFocus(First)` for forward entry or `NavigateFocus(Last)` for
   backward entry.
5. If `WasFocusMoved` is true, treat Tab as handled.
6. If no XAML candidate exists, continue native traversal instead of trapping
   focus.

```csharp
XamlSourceFocusNavigationReason reason = backward
    ? XamlSourceFocusNavigationReason.Last
    : XamlSourceFocusNavigationReason.First;
XamlSourceFocusNavigationRequest request = new(reason);
XamlSourceFocusNavigationResult result = source.NavigateFocus(request);
return result.WasFocusMoved;
```

Calling `NavigateFocus` also lets XAML present keyboard focus visuals. Directly
calling `SetFocus` on the bridge does not carry the same navigation reason.

## Leaving an island

Subscribe to `DesktopWindowXamlSource.TakeFocusRequested`. For `First`/forward or
`Last`/backward requests:

1. Identify the wrapper/tab-stop corresponding to the sender source.
2. Ask native traversal for the next or previous enabled, visible tab stop.
3. Set focus through the native framework.
4. Report or log the transition.

The event can be raised synchronously inside `NavigateFocus` when the island has no
focusable content. Do not recursively call the same request forever.

## Correlation and recursion

Each `XamlSourceFocusNavigationRequest` has a correlation ID. Retain the ID of a
request the host initiated. When a synchronous `TakeFocusRequested` carries that
same ID, the request has returned without finding a XAML target. Continue native
focus or issue a separate `Restore` request according to the framework's policy;
do not reissue the identical `First`/`Last` request.

Deduplicate public "XAML got focus" notifications by correlation ID when both the
synchronous `NavigateFocus` result and the source's `GotFocus` event can report the
same transition.

## `WM_SETFOCUS` fallback

A wrapper can receive focus through mouse, accessibility, explicit `SetFocus`, or
native dialog traversal that bypasses custom Tab handling. Its `WM_SETFOCUS`
handler should call `NavigateFocus` using the pending direction when known and
current Shift state otherwise.

If XAML cannot accept focus, immediately continue native traversal. Returning with
focus parked on an inert wrapper creates a keyboard trap.

## Multiple islands

Maintain a mapping from each native tab-stop/bridge HWND to its source. Native
traversal should see each wrapper once, while XAML handles all internal stops.
Never enumerate XAML children as native dialog controls.

When focus leaves island A for island B, native traversal selects B's wrapper and
then calls B's `NavigateFocus` with the original direction. Keep per-navigation
state thread-local or scoped so reentrant focus events do not overwrite another
top-level window's direction.

## Hidden and disabled hosts

Native dialog traversal should skip a host whose wrapper is hidden or disabled.
When content becomes nonfocusable while the island owns focus, move focus according
to application policy before hiding/disabling the native tab stop.

Test these states explicitly; a source can still hold a XAML focused element even
when native eligibility changes.

## HWND-hosted descendants

Controls such as WebView2 can introduce descendant HWNDs. Treat focus anywhere in
the site-bridge subtree as belonging to the island for top-level routing. Do not
steal focus back to a XAML peer after the user interacts with an HWND-hosted child.

Use `GetAncestor`, parent walking, or the framework's HWND ownership map rather than
checking only exact equality with the site-bridge HWND.

## Inactive top-level windows

Multiple top-level windows can share a XAML thread. A logical XAML focus correction
in an inactive window must not unexpectedly activate it. Test repeated activation
switches and light-dismiss popup closure. Avoid unconditional native `SetFocus`
from generic XAML focus-changed handlers.

## Input semantics beyond Tab

- Space and Enter should activate a focused control once.
- Accelerators should be invoked once.
- Arrow keys intended for XAML controls should remain in the island.
- Popup light-dismiss should return or preserve focus according to control policy.
- IME input belongs to the live XAML editor and needs real interactive testing.

A pretranslator added once per process/thread is safer than one added per host.
Reference-count or otherwise coordinate installation when wrappers can come and go.

## Failure signatures

| Symptom | First check |
| --- | --- |
| Typing works only after mouse click | Host never called `NavigateFocus` on native focus entry. |
| Buttons activate twice | Message was pretranslated and then dispatched again, or two filters ran. |
| Tab lands on wrapper but no XAML element | Direction was lost or only native `SetFocus` was used. |
| Shift+Tab enters the first XAML element | Host always uses `First`; preserve backward direction. |
| Focus cannot leave the last XAML element | `TakeFocusRequested` is missing or native traversal starts from the wrong HWND. |
| Empty island traps focus | `WasFocusMoved == false` is ignored or correlation recursion repeats. |
| Hidden/disabled island receives Tab | Wrapper native style/state is not reflected in traversal. |
| Another window activates unexpectedly | Focus callback unconditionally sets native focus on a shared thread. |
| WebView2 loses focus to XAML parent | Descendant HWND focus is not recognized as island-owned. |

## Acceptance sequence

Automate this order in a fresh process:

1. Native control before island receives focus.
2. Forward Tab enters first XAML element.
3. Forward Tab reaches second XAML element.
4. Forward Tab leaves for native control after island.
5. Forward traversal wraps according to application policy.
6. Backward traversal wraps, enters last XAML element, reaches first, then leaves.
7. Hidden host is skipped.
8. Disabled host is skipped.
9. Repeated top-level reactivation preserves expected native focus.
10. Space, Enter, accelerator, arrow, and popup scenarios deliver once and retain
    the expected focus domain.

The bundled sample does not automate this full matrix. A consuming framework must
supply a subprocess integration test using its own dialog traversal and message
filter.

## Sources

Use the `DesktopWindowXamlSource` focus API, SimpleIslandApp, and WinUI
`xaml-island-focus-navigation.md` entries in [sources.md](sources.md). That design
note includes the native traversal algorithm and relevant XAML focus-manager call
stacks.

## Known gaps

Directional navigation reasons, gamepad/remote input, accessibility-driven focus,
IME candidate windows, and framework-specific accelerator ordering need separate
coverage when the product supports them.
