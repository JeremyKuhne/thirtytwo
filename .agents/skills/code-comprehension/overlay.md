---
core: code-comprehension
core-pin: v0.11.0
---

# Code comprehension overlay

Repository-specific bindings for thirtytwo.

- Product code lives under [src/thirtytwo](../../../src/thirtytwo); tests live
  under [src/thirtytwo_tests](../../../src/thirtytwo_tests), and samples live
  under [src/samples](../../../src/samples).
- Read all files of a partial type together before judging method or type
  complexity. Window, control, dialog, and COM behavior is intentionally split
  by responsibility.
- Treat explicit native ownership, HRESULT handling, ABI types, and lifetime
  scopes as essential complexity. A readability refactor must not hide who
  owns a handle, pointer, COM reference, or native allocation.
- Prefer the established `Windows` and `Windows.Win32` object model over a new
  wrapper layer introduced only to shorten a method.
- Pair readability review with [pre-pr-self-review](../pre-pr-self-review/SKILL.md)
  when changes are proposed.
