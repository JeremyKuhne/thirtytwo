---
core: cswin32-com
core-pin: v0.14.0
---

# CsWin32 COM overlay

Repository-specific bindings for thirtytwo.

## Concrete helpers

- Owned transient COM references use
  [ComScope](../../../src/thirtytwo/Win32/System/Com/ComScope.cs) and its generic
  companion.
- Stable interface IDs use
  [IID.Get<T>()](../../../src/thirtytwo/Win32/Foundation/IID.cs).
- Managed CCWs are implemented by
  [CustomComWrappers](../../../src/thirtytwo/Win32/System/Com/CustomComWrappers.cs)
  and the `IManagedWrapper<T>`/vtable infrastructure in the same directory.
- Long-lived ownership helpers include
  [Lifetime<TVTable, TObject>](../../../src/thirtytwo/Win32/System/Com/Lifetime.cs).
- COM tests live under
  [src/thirtytwo_tests/Win32/System/Com](../../../src/thirtytwo_tests/Win32/System/Com).

## Local rules

- This repository targets only `net10.0-windows`; the portable core's .NET
  Framework branches do not apply.
- Preserve `ComScope<T>`'s owned-reference contract and readonly/no-copy design.
  Pair changes with [il-copy-inspection](../il-copy-inspection/SKILL.md) when a
  ref struct or by-value call shape changes.
- Use [cswin32-interop](../cswin32-interop/SKILL.md) first for generated types,
  blittable signatures, P/Invoke, and byte/element accounting.
- Run [security-review](../security-review/SKILL.md) for vtable, pointer,
  marshalling, or ownership changes.

## Validation

```pwsh
dotnet build src/thirtytwo/thirtytwo.csproj --configuration Release
dotnet test src/thirtytwo_tests/thirtytwo_tests.csproj --configuration Release --no-build --report-trx
```
