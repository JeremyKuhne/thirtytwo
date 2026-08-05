# thirtytwo agent skills

This repository carries 13 portable skill cores from the
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills)
commons, pinned to the immutable `v0.14.0` release. The installed core files
carry `metadata.github-*` provenance injected by `gh skill install` and must
remain exact upstream mirrors. Repository-specific paths and conventions live
in each sibling `overlay.md`.

## Inventory

| Skill | Use in thirtytwo | Local binding |
| --- | --- | --- |
| [address-pr-feedback](address-pr-feedback/SKILL.md) | Address review comments and CI failures on an existing PR. | Uses the repository's Windows build and test gates. |
| [agent-files-review](agent-files-review/SKILL.md) | Review skills, overlays, and other agent customization files. | Validates this catalog with the bundled strict validator. |
| [code-comprehension](code-comprehension/SKILL.md) | Review complex Win32, COM, lifetime, and partial-type code for readability. | Treats ownership and ABI details as essential complexity. |
| [create-pr](create-pr/SKILL.md) | Publish reviewed work as a new PR. | Targets `main` after the local build, test, and skill gates pass. |
| [cswin32-com](cswin32-com/SKILL.md) | Work on raw struct-based COM, CCWs, vtables, and COM lifetime. | Binds to thirtytwo's `ComScope<T>`, `IID`, and `CustomComWrappers` implementations. |
| [cswin32-interop](cswin32-interop/SKILL.md) | Work on CsWin32 projections, native signatures, ownership, and size units. | Binds to the library's `NativeMethods` configuration and public `Interop` extender. |
| [engineering-baseline](engineering-baseline/SKILL.md) | Audit repository engineering and supply-chain practices. | Assesses the existing Windows-only repository; it does not scaffold over it. |
| [github-actions-cost-optimization](github-actions-cost-optimization/SKILL.md) | Analyze GitHub Actions cost without weakening required checks. | Preserves the Windows runner required by product and test behavior. |
| [il-copy-inspection](il-copy-inspection/SKILL.md) | Inspect emitted IL for copies and boxing of structs and ref structs. | Focuses on handles, COM scopes, message views, and buffer scopes. |
| [manage-skills](manage-skills/SKILL.md) | Find, build, review, update, retire, and reconcile skills. | Owns the pinned-core plus local-overlay lifecycle for this catalog. |
| [pre-pr-self-review](pre-pr-self-review/SKILL.md) | Review the working diff before publishing. | Runs Debug and Release checks and invokes security review for unsafe changes. |
| [scratch-buffer-strategy](scratch-buffer-strategy/SKILL.md) | Choose among stack, pooled, and heap scratch buffers. | Binds to `BstrBuffer`, `ValueBuffer<T>`, and the modern-only TFM. |
| [security-review](security-review/SKILL.md) | Audit unsafe code, native boundaries, malformed input, and resource ownership. | Uses the mirrored product/test layout and Windows interop threat surface. |

## Selection boundary

The .NET Framework-only skills (`dotnet-polyfills` and
`framework-jit-optimization`) are not installed because thirtytwo targets only
`net10.0-windows`. `scratch-buffer-strategy` is retained despite its broader
cross-TFM backing data because this repository directly owns stack-backed and
pooled buffer abstractions.

The project-gated `performance-testing`, `fuzz-testing`, and
`roslyn-analyzers` skills are not installed because the repository has no perf,
fuzz, or analyzer project. Add the corresponding project as a separate,
reviewed prerequisite before vendoring one of those cores.

## Disambiguation

### CsWin32 interop and COM

- Use `cswin32-interop` for generated functions, structs, constants, handles,
  native allocation, byte/element accounting, and `NativeMethods` settings.
- Use `cswin32-com` for COM interfaces, IIDs, vtables, activation, CCWs,
  connection points, and reference lifetime.
- When a change touches both, apply the general interop rules first and then
  the COM-specific rules.

### Review and publishing

- Use `pre-pr-self-review` before any initial publish.
- Use `create-pr` when no PR exists for the branch.
- Use `address-pr-feedback` after review comments or CI results exist.
- Run `security-review` alongside the pre-PR review for unsafe, pointer,
  marshalling, parsing, or caller-supplied input changes.

### Skill lifecycle and validation

- Use `manage-skills` to discover, vendor, update, or reconcile a skill.
- Use `agent-files-review` to validate the resulting customization files.

### Repository and workflow audits

- Use `engineering-baseline` for a whole-repository assessment.
- Use `github-actions-cost-optimization` for runner time and workflow cost
  specifically.

### Struct and buffer analysis

- Use `scratch-buffer-strategy` to choose the storage design.
- Use `il-copy-inspection` to inspect what the compiler emitted.
- Use `security-review` to verify pointer, length, ownership, and cleanup
  preconditions.

## Updating

Reinstall each reviewed core from one immutable release, preserving its local
overlay:

```pwsh
gh skill install JeremyKuhne/agent-skills skills/<name> --pin vX.Y.Z --agent github-copilot --scope project --force
```

Install hard dependencies before their consumers: `agent-files-review` before
`manage-skills`, and `cswin32-interop` before `cswin32-com`. Update every
affected overlay's `core-pin` and review the resulting diff. Never hand-edit a
vendored core to resolve drift.

## Validation

Run the bundled strict validator and the reference specification validator:

```pwsh
pwsh -NoProfile -File .agents/skills/manage-skills/scripts/Validate-Skills.ps1 .agents/skills -RequirePortfolioMetadata
Get-ChildItem .agents/skills -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') } | ForEach-Object { npx --yes skills-ref@0.1.5 validate $_.FullName }
git diff --check
```
