---
core: scratch-buffer-strategy
core-pin: v0.14.0
---

# Scratch buffer strategy overlay

Repository-specific bindings for thirtytwo.

- [BstrBuffer](../../../src/thirtytwo/Win32/Foundation/BstrBuffer.cs) seeds a
  `BufferScope<BSTR>` from inline storage and disposes each populated element.
- [ValueBuffer<T>](../../../src/thirtytwo/Support/ValueBuffer.cs) grows from a
  caller-supplied span into an `ArrayPool<byte>` rental and is explicitly
  experimental.
- The repository targets only `net10.0-windows`; ignore net481 crossover data
  when making a local decision.
- There is no dedicated performance project. Do not change thresholds or make
  performance claims from the portable skill's historical measurements alone;
  add a focused, reviewed benchmark prerequisite when evidence is needed.
- Audit checked byte-size arithmetic, alignment, empty spans, pool return on
  every path, and stack-size bounds with
  [security-review](../security-review/SKILL.md).
- Use [il-copy-inspection](../il-copy-inspection/SKILL.md) when changing a ref
  struct or `[NonCopyable]` layout.
