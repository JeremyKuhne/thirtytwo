---
core: il-copy-inspection
core-pin: v0.17.0
---

# IL copy inspection overlay

Repository-specific bindings for thirtytwo.

High-value copy-sensitive surfaces include:

- handle and value projections under
  [src/thirtytwo/Win32](../../../src/thirtytwo/Win32);
- COM scopes and lifetime types under
  [Win32/System/Com](../../../src/thirtytwo/Win32/System/Com);
- stack and pooled buffers under
  [src/thirtytwo/Support](../../../src/thirtytwo/Support);
- readonly message views under
  [src/thirtytwo/Messages](../../../src/thirtytwo/Messages).

Build Release before inspecting emitted IL:

```pwsh
dotnet build src/thirtytwo/thirtytwo.csproj --configuration Release
Get-ChildItem artifacts -Filter thirtytwo.dll -Recurse
```

Use [scratch-buffer-strategy](../scratch-buffer-strategy/SKILL.md) for storage
design and [security-review](../security-review/SKILL.md) for unsafe
preconditions. Use [performance-testing](../performance-testing/SKILL.md) for
runtime and allocation measurements. This repository has no dedicated source
analyzer for copy diagnostics, so do not claim source diagnostics that were not
actually produced.
