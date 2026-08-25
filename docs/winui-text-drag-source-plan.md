# WinUI text drag source plan

**Status:** x64 mouse source baseline complete; mixed product transfer,
hardware, and issue follow-up pending

**Last updated:** 2026-08-19

**Windows App SDK baseline:** 2.3.1

**WinUI source pin:** `29ebf098f70df518b57b754130bc94004be8c6bc`

This document tracks the work required to replace the current experimental
WinUI text drag implementation with a small, evidence-based implementation and
to publish a canonical API guide backed by a build-tested sample.

## Hard constraint

Do not launch a sample, synthesize pointer or keyboard input, or run a test that
creates or interacts with desktop UI without explicit user permission. Build,
lint, link, and other static validation are allowed. Permission for a prior UI
run does not carry forward to a later phase. Every approved run recorded below
consumed one single-use approval. No UI or synthetic-input permission is currently
active.

## Problem statement

The draft that prompted this plan implemented WinUI text drag initiation in a
1,210-line `src/thirtytwo.winui/WinUITextControl.DragDrop.cs`. Phase 0 removed
that file. It manually coordinated:

- island and routed pointer observers;
- pointer capture and terminal events;
- drag thresholds;
- text hit testing and cursor selection;
- transient selection snapshots and dispatcher timing;
- `StartDragAsync` and source completion;
- target routing, insertion geometry, and a composition caret;
- same-source Move transactions and lifecycle cancellation.

RichEditBox drag initiation remains unreliable. The implementation duplicates
behavior already owned by WinUI while depending on event ordering that differs
between TextBox and RichEditBox. It must be treated as an unproven experiment,
not as a compatibility constraint.

## Goals

1. Document the supported WinUI drag-source API and its ownership model.
2. Provide a build-tested raw-HWND island sample using that API.
3. Establish whether an editable TextBox or RichEditBox can itself be a reliable
   drag source without custom pointer tracking.
4. Remove the current source-drag experiment before designing the product API.
5. Add product behavior only after the sample proves it at the native/WinUI
   boundary.

## Non-goals

- Do not preserve the current API merely because it exists in an uncommitted
  draft. There are no compatibility consumers.
- Do not use `InputPointerSource`, custom pointer state, or a direct
  `StartDragAsync` call in the canonical implementation.
- Do not acquire a second `DragDropManager` for ordinary XAML content.
- Do not register an OLE target over XAML's site-bridge HWND.
- Do not include Move semantics in the first canonical sample.
- Do not treat native OLE drag/drop and XAML drag initiation as one abstraction.

## Evidence

### Documented API contract

The Microsoft drag-and-drop guidance describes this source sequence:

1. Set `UIElement.CanDrag` to `true`.
2. Handle `UIElement.DragStarting` to populate `DragStartingEventArgs.Data`.
3. Set `DragStartingEventArgs.AllowedOperations` to the operations the source
   permits.
4. Set `DataPackage.RequestedOperation` to the desired default operation.
5. Optionally call `DragUI.SetContentFromDataPackage()` for a system-provided
   format visual.
6. Handle `UIElement.DropCompleted` when source mutation is required after a
   successful Move.

References:

- [Drag and drop overview](https://learn.microsoft.com/windows/apps/develop/data/drag-and-drop)
- [UIElement.CanDrag](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.candrag)
- [UIElement.DragStarting](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.dragstarting)
- [UIElement.DropCompleted](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.dropcompleted)
- [DragUI.SetContentFromDataPackage](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.dragui.setcontentfromdatapackage)

### Source-observed behavior

At the pinned WinUI commit:

- `AutomaticDragHelper` owns pointer capture, mouse/pen movement, touch holding,
  release, capture loss, and drag initiation for `CanDrag`.
- Its mouse threshold is twice `SM_CXDRAG` and `SM_CYDRAG`.
- It calls `StartDragAsync` after the threshold is crossed.
- `UIElement.ConfigureAutomaticDragHelper` installs that helper when `CanDrag`
  changes.
- The helper subscribes to `PointerPressed` as an ordinary routed handler, so it
      does not receive an event already marked handled.
- `TextBoxBase::TxGetPropertyBits` sets `TXTBIT_DISABLEDRAG`, disabling the
  embedded RichEdit engine's own drag behavior.
- `TextBoxBase::OnPointerPressed` forwards input to RichEdit and marks the routed
  press handled.
- TextBox and RichEditBox do not override
  `ShouldAutomaticDragHelperHandleInputEvents`.

For a direct mouse gesture on either editor in this implementation, the native
RichEdit drag owner is disabled and the XAML automatic helper misses the handled
press it needs to begin tracking. That explains the measured failure on the
pinned package; it is not an API-wide or future-version guarantee. It also
explains why combining custom pointer observers with text-control class handling
is fragile.

Source references:

- [AutomaticDragHelper.cpp](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/AutomaticDragHelper.cpp)
- [UIElement_Partial_DragDrop.cpp](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/UIElement_Partial_DragDrop.cpp)
- [JoltClasses.h](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/dxaml/lib/JoltClasses.h)
- [TextBoxBase.cpp](https://github.com/microsoft/microsoft-ui-xaml/blob/29ebf098f70df518b57b754130bc94004be8c6bc/dxaml/xcp/core/native/text/Controls/TextBoxBase.cpp)

### Official sample behavior

The Microsoft XAML drag-and-drop sample places `CanDrag="True"` on a `Grid`.
The grid contains a TextBox, but a separate visual affordance starts the drag.
Its `DragStarting` handler reads the TextBox value and populates the data
package. The target is a TextBox with routed XAML drop handlers.

No Microsoft sample found during this investigation places `CanDrag` on an
editable TextBox or RichEditBox to drag its current selection directly.

Reference:

- [XAML drag-and-drop sample](https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/XamlDragAndDrop)

### Relevant upstream issues

| Issue | State | Relevance |
| --- | --- | --- |
| [microsoft-ui-xaml#10681](https://github.com/microsoft/microsoft-ui-xaml/issues/10681) | Open | A draggable parent does not initiate when a child control owns the pointer input. |
| [microsoft-ui-xaml#6022](https://github.com/microsoft/microsoft-ui-xaml/issues/6022) | Closed | Calling `StartDragAsync` from `PointerMoved` was questioned in favor of the documented `CanDrag` path. |
| [microsoft-ui-xaml#10144](https://github.com/microsoft/microsoft-ui-xaml/issues/10144) | Open | Custom drag visuals can fail to appear. |
| [microsoft-ui-xaml#7690](https://github.com/microsoft/microsoft-ui-xaml/issues/7690) | Open | XAML drag/drop is unsupported in elevated processes. |

Exact searches for `TextBox CanDrag`, `RichEditBox CanDrag`, and the combination
of the TextBox and drag/drop area labels returned no matching issues. Absence of
an issue is not evidence that direct editable-surface dragging is supported.

## Decisions

1. **The canonical sample is Copy-only.** Move requires a separate editing
   transaction, source-generation validation, and index adjustment. Those
   concerns must not obscure source initiation.
2. **A separate drag handle owns the gesture.** TextBox and RichEditBox provide
   selected text but remain ordinary editors.
3. **The system data-format visual is the default.** Use
   `DragUI.SetContentFromDataPackage()` rather than rendering the control.
4. **Direct editor dragging is a diagnostic experiment.** It is not product or
   guide behavior until both controls pass the declared matrix.
5. **The current WinUI source implementation will be removed before replacement.**
   New code must earn its complexity from measured requirements.
6. **Native OLE work remains separate.** It may continue independently, but it
   does not justify custom pointer handling over a WinUI editor.

## Work plan

### Phase 0: Preserve evidence and remove accidental constraints

- [x] Save the current uncommitted draft as a reviewable patch or named backup.
- [x] Record the current source and test behavior by responsibility, not by
      implementation detail.
- [x] Remove `WinUITextControl.EnableDrag` and its package-consumer usage.
- [x] Delete custom WinUI source pointer, cursor, snapshot, and
      `StartDragAsync` code.
- [x] Delete source-only integration assertions and scenario events.
- [x] Decide separately whether `WinUITextControl.EnableDrop` has a justified,
      independently testable contract; do not retain it merely to keep the
      combined file.
- [x] Keep native OLE source/target work untouched unless its own review finds a
      defect.

**Exit gate:** No product code claims to support dragging selected text from a
WinUI editor, and existing non-drag WinUI behavior still builds.

### Phase 1: Add the canonical guide upstream

Create `skills/winui-win32-hosting/xaml-drag-source.md` in the `agent-skills`
repository.

- [x] State the exact Windows App SDK and deployment baseline.
- [x] Explain `CanDrag`, `DragStarting`, `AllowedOperations`,
      `RequestedOperation`, `DragUI`, and `DropCompleted` ownership.
- [x] Show the Copy-only explicit-handle pattern.
- [x] Show TextBox and RichEditBox selected-text extraction.
- [x] Explain why the first sample does not offer Move.
- [x] Mark direct editor `CanDrag` behavior as unverified.
- [x] Document elevation, handled-child input, drag-visual, and lifecycle failure
      signatures.
- [x] Link the existing mixed OLE/XAML and island pointer guides without merging
      their implementation layers.
- [x] Update `SKILL.md`, `sources.md`, `mixed-ole-and-xaml-drag-drop.md`, and
      `documentation-roadmap.md`.

**Exit gate:** Markdown lint, offline relative-link validation, portfolio
validation, and `skills-ref` validation pass.

### Phase 2: Add a build-tested canonical sample

Extend `skills/winui-win32-hosting/assets/minimal-host` with a focused text drag
scenario rather than creating another host implementation.

The sample content will contain:

- a TextBox with selectable text;
- a RichEditBox with selectable text;
- one explicit draggable handle beside each editor;
- a non-null-background XAML drop target;
- a small status element for `DropCompleted` and received text.

Each handle will use this shape:

```csharp
handle.CanDrag = true;
handle.DragStarting += (_, eventArgs) =>
{
    string selectedText = GetSelectedText(editor);
    if (selectedText.Length == 0)
    {
        eventArgs.Cancel = true;
        return;
    }

    eventArgs.Data.SetText(selectedText);
    eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
    eventArgs.AllowedOperations = DataPackageOperation.Copy;
    eventArgs.DragUI.SetContentFromDataPackage();
};
```

The sample must not contain:

- `InputPointerSource`;
- routed pointer handlers;
- manual capture or movement thresholds;
- direct `StartDragAsync` calls;
- selection snapshots;
- source deletion or Move transactions;
- a custom `DragDropManager` or OLE registration.

Tasks:

- [x] Extract content creation from the host bootstrap into a focused helper.
- [x] Implement TextBox selection retrieval with `SelectedText`.
- [x] Implement RichEditBox selection retrieval with
      `Document.Selection.Text`.
- [x] Cancel empty-selection drags.
- [x] Implement Copy-only `DragOver` and asynchronous `Drop` with a deferral.
- [x] Record completion without mutating the source.
- [x] Update the sample README with build commands and a permission-gated manual
      matrix.
- [x] Update `tests/winui-win32-hosting/Priority0.Tests.ps1` to include the new
      source file and retain x64/ARM64 build coverage.

**Exit gate:** Release builds pass for `win-x64` and `win-arm64`. No UI is
launched as part of this gate.

### Phase 3: Isolate direct editable-surface behavior

Create a minimal diagnostic repro only after the canonical sample builds. It
must compare these source elements without custom gesture code:

1. TextBlock with `CanDrag`.
2. TextBox with a nonempty selection and `CanDrag`.
3. RichEditBox with a nonempty selection and `CanDrag`.

Run the same cases in:

- an ordinary WinUI top-level window; and
- a raw `DesktopWindowXamlSource` host.

Capture only these transitions:

- press received by the control;
- `DragStarting` raised;
- `DropCompleted` raised;
- final selection and text.

- [x] Build the diagnostic repro without launching it.
- [x] Obtain explicit permission before any manual or automated UI run.
- [x] Run the mouse matrix after permission.
- [ ] Add touch and pen only when suitable hardware and permission are
      available.
- [x] Distinguish general WinUI behavior from island-specific behavior.
- [ ] File a focused upstream issue if TextBox or RichEditBox fails while the
      TextBlock control case succeeds.

Issue creation is externally blocked: GitHub returned Microsoft organization
SAML enforcement errors for issue types and issue creation. The complete draft
is retained at `artifacts/winui-text-drag/upstream-issue.md`.

**Exit gate:** Direct editor behavior is either reproducibly supported on both
hosts or documented upstream with a minimal repro. An intermittent pass does
not satisfy this gate.

### Phase 4: Design the thirtytwo product surface

Choose the product shape from Phase 3 evidence.

#### If direct editor `CanDrag` is reliable

- [ ] Add a narrow source API that forwards `CanDrag`.
- [ ] Populate selected text in `DragStarting`.
- [ ] Keep the first release Copy-only.
- [ ] Use `DropCompleted` only for status and cleanup.
- [ ] Add lifecycle cleanup for event subscriptions.
- [ ] Keep implementation size proportional to this contract; no custom pointer
      state machine is permitted.

#### If direct editor `CanDrag` is unreliable

- [x] Do not restore `WinUITextControl.EnableDrag`.
- [x] Document the explicit-handle pattern for applications.
- [x] Defer a reusable drag-handle helper until multiple samples establish
      real demand.
- [x] Track the upstream issue draft rather than embedding a platform
      workaround in the control wrapper.

#### Move semantics, if requested later

- [ ] Specify Move as a separate API and transaction.
- [ ] Snapshot source range and generation at `DragStarting`.
- [ ] Delete only after `DropCompleted` reports Move.
- [ ] Handle same-editor insertion and adjusted source indices.
- [ ] Keep the inserted copy when source validation fails.
- [ ] Test cancellation, reentrancy, source edits, teardown, and cross-control
      completion independently from drag initiation.

**Exit gate:** The public API follows measured platform behavior, has a small
implementation, and does not depend on undocumented pointer-event ordering.

## Validation matrix

### Static and build checks allowed without additional permission

- [x] Build the upstream sample for x64 and ARM64.
- [x] Build affected thirtytwo projects in Debug and Release.
- [x] Run compiler, analyzer, formatting, Markdown, and link validation.
- [x] Run non-UI unit tests that exercise pure transaction or payload logic.
- [x] Confirm the final diff contains no custom WinUI pointer state machine.

### UI checks requiring explicit permission

- [x] Launch either sample or diagnostic executable.
- [x] Initiate mouse drags. Touch and pen remain pending suitable hardware.
- [x] Send synthetic pointer and keyboard input.
- [x] Run integration tests that create real windows.
- [x] Capture screenshots, UI Automation trees, and live event traces.

When permission is granted, record the exact package version, host topology,
architecture, OS build, input device, source/target pair, effect, and outcome.

## Completion criteria

This plan is complete when:

1. The current custom WinUI source-drag implementation is gone.
2. The upstream canonical guide clearly separates supported API from measured
   and unresolved behavior.
3. The raw-HWND sample builds for x64 and ARM64 and uses only `CanDrag` and
   routed drag/drop APIs.
4. Direct TextBox and RichEditBox behavior has a reproducible result or an
   upstream issue.
5. Thirtytwo exposes only behavior justified by that evidence.
6. Move support, if any, is reviewed as a separate editing transaction.
7. All UI validation performed during the work was explicitly authorized and
   recorded.

These criteria are complete for the measured x64 mouse baseline. Publishing the
upstream issue and extending the hardware/version matrix remain follow-up work.

## Progress log

### 2026-08-11

- Completed API, source, official-sample, issue, and current-code
      investigation.
- Chose a Copy-only explicit-handle sample as the canonical baseline.
- Removed the custom WinUI drag source and retained routed drop behavior in a
      target-only file.
- Added the canonical guide, raw-host sample, and ordinary-Window/raw-island
      direct-editor diagnostic to `agent-skills`.
- Passed strict skill validation and five focused Pester asset tests, including
      Release x64 and ARM64 builds for both host topologies.
- Applied the security review's Option A: RichEdit insertion bounds now use
      nonallocating `StoryLength - 1` and are revalidated immediately before
      mutation.
- Passed the full thirtytwo Release build and package-isolation check. No UI
      executable or real-window test had been run at that stage.
- Received explicit permission and ran the reusable x64 mouse automation matrix
      on Windows App SDK 2.3.1, .NET 10.0.9, Windows build 26200, non-elevated.
- Passed nine UI cases: TextBlock Copy in both hosts; direct TextBox and
      RichEditBox failure with selection behavior in both hosts; canonical TextBox
      and RichEditBox handle Copy; and empty-selection cancellation.
- Retained the machine-readable report at
      `artifacts/winui-text-drag/automated/result.json` and the system-preview image
      at `artifacts/winui-text-drag/canonical-system-preview.png`.
- Confirmed the preview is a system document-format icon with a Copy glyph, not
      an editor snapshot.
- Passed the focused real-window WinUI target configuration and lifecycle
      scenario, including nonallocating TextBox and RichEditBox insertion-bound
      assertions. That scenario does not deliver a drag payload or prove routed
      `Drop` transfer.
- Attempted to file the direct-editor issue after three zero-result duplicate
      searches. GitHub creation is blocked until this token receives Microsoft SAML
      authorization; the complete issue body is retained with the artifacts.

### 2026-08-12

- Triaged two independent pre-PR reviews against the implementation and Win32
      contracts. The proposed `DestroyCaret` fix was rejected: `CreateCaret`
      replaces the queue's prior caret shape, the current visibility calls are
      balanced, and destroying the caret while the Edit control remains focused
      would remove its current queue caret. Reparent recovery, COM callback
      lifetime, same-source boundaries, post-await bounds, and click-collapse
      findings were also contradicted by the controlling code or focused tests.
- Confirmed the portability review's central-package-management concern with a
      direct vendored build. Both standalone projects now opt out of inherited
      CPM and use an asset-local `Directory.Build.props` boundary; both direct
      builds pass beneath thirtytwo's build-policy hierarchy.
- Clarified Copy-only completion ownership and post-await insertion
      revalidation, allowed `win-arm64` runner selection with an ARM64-process
      execution guard, removed an unused `.gitignore` from runtime staging, and
      ignored local diagnostic build outputs.
- The first final vendored run exposed coalesced synthetic source movement. Added
      a bounded three-attempt source-initiation loop that releases and recenters
      between attempts, matching the existing bounded target-entry strategy.
- Retained five post-fix nine-case reports in `final-vendored`, `stability/run-1`
      through `run-3`, and `final-portable`. All 45 recorded cases passed across
      ordinary Window and raw-island control cases, both canonical handles, and
      empty-selection cancellation.
- Passed seven focused Pester tests, including Release x64 and ARM64 compilation
      for both samples; strict validation of all 19 upstream and 14 vendored
      skills; both `skills-ref` validations; the full Release solution build;
      package isolation; diff hygiene; and byte parity for all 16 synchronized
      portable files.
- The earlier 2026-08-11 full product-suite run recorded 490 passed and one
      intentional manual skip. The clean-checkout-equivalent upstream skill run
      recorded 110 passed and one platform skip. These historical counts were
      not rerun as part of the five post-fix UI reports.
- Added the build-only `host-native-text-drag` subprocess scenario. It uses the
      product native `TextDragSource` / OLE path and requires exact target commit
      plus source-owned Move deletion for both WinUI text wrappers. The Release
      integration slice builds, and five non-UI threshold and runner-cleanup
      tests pass.

### 2026-08-19

- Received and consumed explicit permission for one x64 execution of
      `host-native-text-drag`. The process timed out after 20.128 seconds; its
      last event was `native-text-drag-textbox-source-ready`, stderr was empty,
      and neither target entry/commit nor source deletion was recorded. The
      retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Release/host-native-text-drag-0966191287ab43e786187c5be4582d5b/`.
- Replaced the single five-input `SendInput` batch with bounded phases that wait
      for native source positioning, left-button down, held-button movement, and
      routed target entry before release. The x64 Release host and test projects
      build with no diagnostics.
- Received and consumed explicit permission for one phased x64 Debug run. It
      completed in 2.707 seconds and recorded native source positioning,
      left-button down, held threshold movement, and routed TextBox `DragEnter`.
      The harness then observed `AcceptedOperation=None` in its own routed
      handler and aborted before button release, target commit, or source
      deletion. The retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-bf0b142575a7469e9d1a987c5154f35b/`.
- The `None` observation did not establish the wrapper's final decision because
      handler order was not controlled. Deferred the acceptance snapshot until
      after routed-event dispatch and added `Contains(Text)`, allowed-operation,
      modifier, and accepted-operation diagnostics.
- Received and consumed explicit permission for one corrected-observer x64 Debug
      run. It completed in 1.005 seconds and recorded
      `ContainsText=True; Allowed=Copy, Move; Modifiers=LeftButton; Accepted=None`
      after routed-event dispatch. It aborted before release, target commit, or
      source deletion. The retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-c3e219804aac48c4a0fe70770451b570/`.
- Traced rejection to the empty TextBox caret path. In WinUI source commit
      `29ebf098f70df518b57b754130bc94004be8c6bc`,
      `CTextBox::GetRectFromCharacterIndex` rejects every index when text is
      empty; the wrapper requested index `-1` for insertion index zero, caught
      the resulting failure, and set `AcceptedOperation=None`. The earlier
      lifecycle fixture used nonempty text and could not expose this boundary.
- Kept drop tracking active while omitting unsupported positional caret geometry
      for an empty TextBox, where zero is the sole insertion point. Added an
      empty-target regression to the existing host-drop scenario. The six
      focused non-UI checks pass. The x64 Release build is clean; ARM64 has the
      compile-only warning status below.
- Received and consumed explicit permission for one x64 Debug host-drop run. It
      passed in 0.945 seconds, including `winui-empty-text-drop-caret-bounded`,
      and exited cleanly. The retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-drop-target-11fadfd28a4b4765abaa8bdb230133c5/`.
- Received and consumed explicit permission for one post-fix x64 Debug native
      Move run. It recorded
      `ContainsText=True; Allowed=Copy, Move; Modifiers=LeftButton; Accepted=Move`,
      then timed out after 20.123 seconds with DragEnter still the last event. No
      Drop, target commit, or source deletion was observed. The retained result
      is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-acac7a935dab4ea6bde2f0dfa47774b6/`.
- Added explicit mouse-up injection and routed Drop trace points because the
      fourth artifact cannot distinguish a background input-task stall from an
      injected mouse-up that did not produce Drop. The trace contract compiles
      cleanly on x64. ARM64 compilation succeeds with the repository's existing
      `CS8012` warnings because the .NET 10 `KlutzyNinja.Madowaku` 0.5.0 assets
      identify as AMD64; this does not establish native ARM64 execution.
- Received and consumed explicit permission for one handoff-traced x64 Debug
      native Move run. Mouse-up injection returned, and routed TextBox `Drop`
      followed with `Allowed=Copy, Move; Modifiers=None; Accepted=Move`. The
      process then timed out after 20.146 seconds with no target commit, source
      deletion, or stderr. The retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-1a62086ad98f48feb6b1dbcefcc451e3/`.
- The fifth run establishes the boundary at or after routed Drop, not successful
      transfer. The next product operation is `DataView.GetTextAsync`; the run did
      not establish whether that operation started, completed, or invoked native
      `IDataObject.QueryGetData` / `GetData`.
- Added inert phase tracing around product Drop entry, deferral acquisition,
      `GetTextAsync`, deferral completion, and native data-object retrieval. The
      x64 build passes, and ARM64 has the compile-only status above. This deeper
      trace required fresh explicit permission.
- Received and consumed explicit permission for one retrieval-traced x64 Debug
      native Move run. It recorded product Drop entry, deferral acquisition,
      `GetTextAsync` start, routed Drop propagation, and native `QueryGetData`
      and `GetData` entry and return, all on the main STA. `GetData` returned in
      about 0.44 ms, but `GetTextAsync` did not record completion. The process
      timed out after 20.155 seconds with no target mutation, deferral completion,
      OLE Move return, source deletion, or stderr. The retained result is under
      `artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-4da02633559d49bd9b64be6759fb47c0/`.
- The sixth run narrows the observed boundary to after native `GetData` returned
      and before `GetTextAsync` completed. It rules out failure to enter the data
      object callback, but it does not by itself establish the callback HRESULT
      or prove why the WinRT operation did not complete. The temporary product
      observers used for that run were removed. A test-owned `IDataObject`
      adapter now confirms that the focused data-object path returns success,
      but that does not establish the HRESULT from the retained mixed run.
