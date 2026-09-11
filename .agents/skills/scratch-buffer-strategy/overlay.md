---
core: scratch-buffer-strategy
core-pin: v0.17.0
---

# Scratch buffer strategy overlay

Repository-specific bindings for thirtytwo.

- [BstrBuffer](../../../src/thirtytwo/Win32/Foundation/BstrBuffer.cs) seeds a
  `BufferScope<BSTR>` from inline storage and disposes each populated element.
- [`ValueBuffer<T>`](../../../src/thirtytwo/Support/ValueBuffer.cs) grows from a
  caller-supplied span into an `ArrayPool<byte>` rental and is explicitly
  experimental.
- The repository targets only `net10.0-windows`; ignore net481 crossover data
  when making a local decision.
- Validate local thresholds and performance claims with a focused benchmark in
  [thirtytwo.perf](../../../thirtytwo.perf/thirtytwo.perf.csproj); do not rely
  on the portable skill's historical measurements alone. Follow
  [performance-testing](../performance-testing/SKILL.md) for benchmark design
  and execution.
- Audit checked byte-size arithmetic, alignment, empty spans, pool return on
  every path, and stack-size bounds with
  [security-review](../security-review/SKILL.md).
- Use [il-copy-inspection](../il-copy-inspection/SKILL.md) when changing a ref
  struct or `[NonCopyable]` layout.
