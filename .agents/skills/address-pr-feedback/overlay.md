---
core: address-pr-feedback
core-pin: v0.14.0
---

# Address PR feedback overlay

Repository-specific bindings for thirtytwo.

- Read all unresolved review threads and the Windows build status before
  editing. The workflow is
  [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml).
- Re-run the narrowest check that reproduces a finding, then run the Release
  build and tests before marking the work complete:

```pwsh
dotnet build --configuration Release
dotnet test --configuration Release --no-build --report-trx
```

- Re-run [pre-pr-self-review](../pre-pr-self-review/SKILL.md) after substantive
  fixes and [agent-files-review](../agent-files-review/SKILL.md) after changes
  under `.agents/`.
- Keep edits local until the user explicitly approves commit or push actions.
