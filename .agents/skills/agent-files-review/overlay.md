---
core: agent-files-review
core-pin: v0.11.0
---

# Agent files review overlay

Repository-specific bindings for thirtytwo.

The current agent surface is `.agents/skills/`. This repository has not adopted
an `AGENTS.md` plus `.github/copilot-instructions.md` mirror or a dedicated
agent-files workflow, so do not claim those gates exist. If that scaffold is
added later, update this overlay with its exact commands.

Validate the installed portfolio with:

```pwsh
pwsh -NoProfile -File .agents/skills/manage-skills/scripts/Validate-Skills.ps1 .agents/skills -RequirePortfolioMetadata
Get-ChildItem .agents/skills -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') } | ForEach-Object { npx --yes skills-ref@0.1.5 validate $_.FullName }
git diff --check
```

Review local files against the [format contract](../FORMAT.md) and keep the
[catalog](../README.md) synchronized with the skill directories. Vendored core
files must match their `metadata.github-*` provenance; local changes belong in
overlays.
