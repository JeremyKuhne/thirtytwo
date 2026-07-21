---
core: cswin32-interop
core-pin: v0.11.0
---

# CsWin32 interop overlay

Repository-specific bindings for thirtytwo.

## Surface

- The owning project is
  [src/thirtytwo/thirtytwo.csproj](../../../src/thirtytwo/thirtytwo.csproj),
  targeting `net10.0-windows` with C# 14.
- CsWin32 inputs are
  [NativeMethods.txt](../../../src/thirtytwo/NativeMethods.txt) and
  [NativeMethods.json](../../../src/thirtytwo/NativeMethods.json).
- The generator emits a public `Interop` class whose methods are also
  accessible on `PInvoke` as extension methods, with `allowMarshaling: false`,
  `useSafeHandles: false`, and preserved COM signatures. Keep native
  signatures blittable.
- Generated and hand-authored projections use the `Windows.Win32` namespace.
  Search the existing generated input and partial types before adding a local
  duplicate.
- The repository is modern-only and Windows-only. Framework TFM branches and
  cross-platform runtime guards in the portable core do not apply unless the
  project targets change.

## Ownership and exceptions

- Preserve explicit handle, allocator, pointer, and byte-versus-element
  contracts. Pair changes involving unsafe preconditions with
  [security-review](../security-review/SKILL.md).
- The hand-written `NtQueryKey` declaration in
  [Interop.NtQueryKey.cs](../../../src/thirtytwo/Wdk/Interop.NtQueryKey.cs)
  covers a metadata gap. Do not replace it without verifying the available
  Win32 metadata and the existing analyzer suppression rationale in
  [AssemblyAttributes.cs](../../../src/thirtytwo/AssemblyAttributes.cs).
- Use [cswin32-com](../cswin32-com/SKILL.md) for COM vtables, activation,
  CCWs, and reference lifetime.

## Validation

```pwsh
dotnet build src/thirtytwo/thirtytwo.csproj --configuration Release
dotnet test src/thirtytwo_tests/thirtytwo_tests.csproj --configuration Release --no-build --report-trx
```
