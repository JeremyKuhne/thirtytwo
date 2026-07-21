---
core: security-review
core-pin: v0.11.0
---

# Security review overlay

Repository-specific bindings for thirtytwo.

- Product and test code are mirrored under
  [src/thirtytwo](../../../src/thirtytwo) and
  [src/thirtytwo_tests](../../../src/thirtytwo_tests).
- Treat all raw Win32 calls, COM vtables, CCWs, handles, native allocations,
  `unsafe`, `Unsafe`, `MemoryMarshal`, pointer arithmetic, and stack or pooled
  buffers as caller-validated surfaces.
- Verify HRESULT and last-error behavior, owned versus borrowed handles and COM
  references, allocator/deallocator pairing, byte-versus-element units,
  checked size arithmetic, null and empty spans, and cleanup on every failure
  path.
- Place regression tests in the existing mirrored test area for the production
  type. Use a dedicated `.Security.cs` file only when it improves the local
  test organization.
- Run focused tests first, then the Release suite:

```pwsh
dotnet test src/thirtytwo_tests/thirtytwo_tests.csproj --configuration Release --report-trx
```

Use [cswin32-interop](../cswin32-interop/SKILL.md) for general native
signatures and [cswin32-com](../cswin32-com/SKILL.md) for COM-specific lifetime
and ABI rules.
