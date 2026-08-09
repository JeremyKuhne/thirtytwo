# Agent Skills format reference

This file defines the downstream conventions for `.agents/skills/`. See
[README.md](README.md) for the installed inventory and selection boundary.

## Layout

```text
.agents/skills/
  README.md
  FORMAT.md
  <skill-name>/
    SKILL.md
    overlay.md
    <vendored resources>
```

The directory name must match the `name` field in `SKILL.md` exactly.

## Vendored cores

A core installed from the commons includes source tracking in frontmatter:

```yaml
metadata:
  github-path: skills/<skill-name>
  github-pinned: vX.Y.Z
  github-ref: refs/tags/vX.Y.Z
  github-repo: https://github.com/JeremyKuhne/agent-skills
  github-tree-sha: <tree-sha>
```

The core and its bundled siblings are immutable mirrors of that pin. Do not
edit them directly. Reinstall from a reviewed immutable ref when updating.

## Local overlays

Repository-specific paths, commands, examples, cross-references, and policy
belong in `overlay.md` beside the core:

```markdown
---
core: skill-name
core-pin: vX.Y.Z
---

# Skill name overlay
```

The `core` value must match the directory name. Update `core-pin` only after
reviewing the overlay against the new core.

## Catalog

Every installed skill must have one row in [README.md](README.md) describing
why it applies here and what its overlay binds. Record project-gated and
TFM-specific exclusions so future additions are deliberate.

## Markdown

- Use relative links for repository files.
- Do not use HTML entities; write the character directly or use plain words.
- Do not use tabs, trailing whitespace, or whitespace-only lines.
- Give every fenced code block a language tag.
- End every file with one newline.

## Validation

```pwsh
pwsh -NoProfile -File .agents/skills/manage-skills/scripts/Validate-Skills.ps1 .agents/skills -RequirePortfolioMetadata
Get-ChildItem .agents/skills -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') } | ForEach-Object { npx --yes skills-ref@0.1.5 validate $_.FullName }
git diff --check
```
