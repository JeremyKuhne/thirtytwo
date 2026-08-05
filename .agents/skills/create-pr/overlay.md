---
core: create-pr
core-pin: v0.14.0
---

# Create PR overlay

Repository-specific bindings for thirtytwo.

- The canonical repository is `JeremyKuhne/thirtytwo`; PRs target `main`.
- Run [pre-pr-self-review](../pre-pr-self-review/SKILL.md) before publishing.
- The required product gate is the Windows workflow in
  [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml).
- Before publishing, build and test the current tree in Release:

```pwsh
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --report-trx
```

- When `.agents/` changes, also run the commands in
  [agent-files-review](../agent-files-review/SKILL.md).
- Stage by path and do not publish unrelated working-tree changes.
