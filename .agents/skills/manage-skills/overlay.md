---
core: manage-skills
core-pin: v0.11.0
---

# Manage skills overlay

Repository-specific bindings for thirtytwo.

## Ownership

- Every provenance-bearing core under `.agents/skills/` comes from
  `JeremyKuhne/agent-skills` and must remain identical to its pinned upstream
  artifact.
- Put thirtytwo paths, commands, examples, and cross-skill links in
  `overlay.md`. The local [catalog](../README.md) and
  [format contract](../FORMAT.md) are also downstream-owned.
- The current portfolio is reviewed against the immutable `v0.11.0` release.

## Pulling a commons release

Use the exact source path and an immutable pin:

```pwsh
gh skill install JeremyKuhne/agent-skills skills/<name> --pin vX.Y.Z --agent github-copilot --scope project --force
```

Install `cswin32-interop` before its required consumer `cswin32-com`. Preserve
the local overlay, update its `core-pin`, update the catalog when the portfolio
changes, and review the normal diff.

## Validation

```pwsh
pwsh -NoProfile -File .agents/skills/manage-skills/scripts/Validate-Skills.ps1 .agents/skills -RequirePortfolioMetadata
Get-ChildItem .agents/skills -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') } | ForEach-Object { npx --yes skills-ref@0.1.5 validate $_.FullName }
git diff --check
```

Do not vendor a project-gated skill until its required perf, fuzz, or analyzer
project exists or is being added in the same reviewed effort.
