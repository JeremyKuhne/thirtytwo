---
core: pre-pr-self-review
core-pin: v0.11.0
---

# Pre-PR self-review overlay

Repository-specific bindings for thirtytwo.

- Product code is [src/thirtytwo](../../../src/thirtytwo); tests are
  [src/thirtytwo_tests](../../../src/thirtytwo_tests), with samples under
  [src/samples](../../../src/samples).
- The repository targets only `net10.0-windows`. Skip the portable core's
  .NET Framework and polyfill checks unless target frameworks change.
- Use a read-only review pass over the complete diff; no dedicated local
  reviewer agent is currently installed.
- Invoke [security-review](../security-review/SKILL.md) for changes involving
  `unsafe`, pointers, `Unsafe`, `MemoryMarshal`, `Marshal`, `stackalloc`, COM,
  P/Invoke, native ownership, or caller-supplied buffers and lengths.
- Invoke [agent-files-review](../agent-files-review/SKILL.md) for changes under
  `.agents/`.

Run both configurations before publishing:

```pwsh
dotnet build --configuration Debug
dotnet test --configuration Debug --no-build --report-trx
dotnet build --configuration Release
dotnet test --configuration Release --no-build --report-trx
git diff --check
```

Test names should identify the member under test, the relevant state, and the
expected behavior. Dispose native and managed resources even when an assertion
fails.
