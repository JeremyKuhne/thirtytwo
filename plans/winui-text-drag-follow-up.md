# WinUI text drag/drop agent follow-up

**Checkpoint date:** 2026-08-25

**Branch at checkpoint:** `main`

**Windows App SDK:** 2.3.1

**WinUI source pin:** `29ebf098f70df518b57b754130bc94004be8c6bc`

**Measured runtime:** Windows build 26200, x64, mouse

This document transfers the current WinUI text drag/drop work to another agent
or machine. Start with the commit containing this file. The implementation,
tests, samples, skill guidance, and the longer investigation record are all in
that commit.

## Non-negotiable constraints

- Do not launch a sample, move the system cursor, inject input, or run a UI test
  without fresh, explicit, single-use permission from the user. No permission is
  active at this checkpoint.
- Never add product code solely for tests or diagnostics. Put observation and
  substitution in test-owned adapters, dedicated test oracles, debugger probes,
  or external diagnostics.
- Do not add `IAgileObject`, `IMarshal`, or `IDataObjectAsyncCapability` based
  only on the current stall. Ordinary synchronous OLE extraction does not
  require `IDataObjectAsyncCapability`.
- Keep direct editable-surface initiation separate from native OLE-to-XAML
  transfer. They have different owners and failure modes.
- Preserve the existing loss-averse Move contract: delete source text only
  after the destination commits and OLE returns Move.

## Implemented scope

- Native OLE drag/drop primitives and text data transfer live under
  [src/thirtytwo](../src/thirtytwo), including `DragSource`, `DropTarget`,
  `DropDataObject`, `UnicodeTextDataObject`, `TextDragSource`,
  `TextDragSession`, and `TextDropTarget`.
- Native edit controls expose selected-text drag initiation and conservative
  source deletion after a returned Move.
- [WinUITextControl.Drop.cs](../src/thirtytwo.winui/WinUITextControl.Drop.cs)
  implements routed XAML text-drop acceptance, insertion tracking, asynchronous
  retrieval, target mutation, and deferral completion.
- An empty TextBox keeps insertion tracking but omits unsupported character
  geometry. Its regression passed in the authorized host-drop run.
- [TextControlComparisonWindow.cs](../src/samples/WinUI/TextControlComparison/TextControlComparisonWindow.cs)
  uses explicit Copy-only drag handles. Direct dragging from TextBox and
  RichEditBox editable surfaces is not established.
- [NativeTextDragScenario.cs](../src/thirtytwo.winui_tests/IntegrationHost/NativeTextDragScenario.cs)
  provides the bounded native Move scenario. Its controller assertion now uses
  only behavior-level events: routed DragEnter/Drop, target commit, OLE Move
  return, source deletion, and final text verification.
- [WindowDropTargetTests.cs](../src/thirtytwo_tests/WindowDropTargetTests.cs)
  uses a test-owned forwarding `IDataObject` adapter for focused call-order and
  successful-HRESULT evidence. Product classes contain no test observers.
- The canonical guidance and build-tested assets live in
  [.agents/skills/winui-win32-hosting](../.agents/skills/winui-win32-hosting).
  The full investigation log is
  [winui-text-drag-source-plan.md](../docs/winui-text-drag-source-plan.md).

## Established runtime evidence

The explicit-handle XAML Copy baseline works on the measured x64 mouse setup.
Direct `CanDrag` initiation on either editable control does not. Source review
explains the measured result: text-control class handling consumes the press
before `AutomaticDragHelper` can begin tracking. This is evidence for the pinned
package, not a guarantee for every Windows App SDK version or input device.

Six separately authorized native Move runs progressively established:

1. The source reached readiness.
2. Native button-down and threshold movement reached routed TextBox DragEnter.
3. The initial empty TextBox target rejected the drag because its wrapper asked
   for unsupported character geometry.
4. After the empty-target fix, DragEnter accepted Move.
5. Mouse-up injection returned and routed TextBox Drop ran with Copy and Move
   allowed, no modifiers, and Move accepted.
6. Temporary diagnostics recorded product Drop entry, deferral acquisition,
   `DataPackageView.GetTextAsync` start, routed Drop propagation, and native
   `IDataObject.QueryGetData` and `GetData` entry and return on the main STA.

The sixth run timed out after 20.155 seconds. `GetData` returned in about
0.44 ms, but the trace did not record its HRESULT. `GetTextAsync` did not record
completion. There was no target mutation, deferral completion, OLE Move return,
source deletion, or stderr. The last event was the native `GetData` return.

The original local artifact was:

```text
artifacts/test-results/WinUIIntegrationHarness/Debug/host-native-text-drag-4da02633559d49bd9b64be6759fb47c0/
```

`artifacts/` is not part of this transfer. The facts needed to continue are
captured above. Temporary product observers used for that run were subsequently
removed. The focused test-owned adapter proves that the ordinary product data
object path returns success, but it does not retroactively establish the HRESULT
from the mixed run.

## Unresolved behavior

- Why the OLE-to-WinUI transfer remains pending after native `GetData` returns.
- Whether `GetTextAsync` completes in the mixed TextBox scenario.
- Target mutation, Drop deferral completion, OLE returning Move, and source
  deletion in that scenario.
- The RichEditBox mixed-transfer path, which starts only after TextBox succeeds.
- ARM64 execution, touch, pen, external targets, and broader device coverage.

## Recommended next investigation

1. Reproduce the static baseline with the commands below.
2. Review the retained facts and current platform/source contracts before
   changing product behavior.
3. For another runtime trace, obtain fresh single-use permission first.
4. Prefer debugger breakpoints, ETW/platform diagnostics, or a dedicated
   test-owned OLE/XAML oracle that can record the returned HRESULT and ownership
   transitions. Do not restore observer fields or injectable factories to
   shipping types.
5. Use the next trace to discriminate among a failed `GetData` result, medium
   ownership/consumption, broker conversion, and WinRT operation completion.
6. Change product code only after that evidence identifies a violated contract;
   then add a behavior-level regression that fails without the fix.

## Static validation at checkpoint

The following completed without launching UI or injecting input:

- Six focused non-UI tests passed: the Unicode data object, drag threshold, and
  four integration-runner cleanup/exit-code tests.
- The x64 Release project graph built with no warnings or errors.
- The ARM64 Release project graph compiled with five `CS8012` warnings, all from
  the known AMD64 Madowaku reference mismatch. ARM64 execution is not proven.
- Editor diagnostics were clean for all touched product and test files.
- Both skill validators accepted all 14 installed skills.
- `git diff --check` passed.

Re-run the build and skill checks from the repository root:

```pwsh
dotnet build src/thirtytwo_tests/thirtytwo_tests.csproj -c Release -p:Platform=x64 -p:PlatformTarget=x64 -p:RuntimeIdentifier=win-x64
dotnet build src/thirtytwo_tests/thirtytwo_tests.csproj -c Release -p:Platform=ARM64 -p:PlatformTarget=ARM64 -p:RuntimeIdentifier=win-arm64
pwsh -NoProfile -File .agents/skills/manage-skills/scripts/Validate-Skills.ps1 .agents/skills -RequirePortfolioMetadata
Get-ChildItem .agents/skills -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') } | ForEach-Object { npx --yes skills-ref@0.1.5 validate $_.FullName }
git diff --check
```

The ARM64 build changes shared restore assets. Restore/build x64 again before
running x64 tests. Do not run the full integration suite without reviewing its
scenarios and obtaining permission for any test that launches UI or injects
input.