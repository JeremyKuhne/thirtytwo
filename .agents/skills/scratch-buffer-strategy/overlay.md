---
core: scratch-buffer-strategy
core-pin: v0.11.0
---

# Scratch buffer strategy overlay

Repository-specific bindings for thirtytwo.

- [StackBufferScope16<T>](../../../src/thirtytwo/Support/StackBufferScope16.cs)
  provides 16 inline elements and falls back through `BufferScope<T>`.
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
