---
core: github-actions-cost-optimization
core-pin: v0.14.0
---

# GitHub Actions cost optimization overlay

Repository-specific bindings for thirtytwo.

- The current workflow surface is
  [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml).
- It gates pushes and pull requests to `main` and performs restore, Debug and
  Release builds, Release tests, and TRX upload.
- Keep a Windows runner for product validation: the library targets
  `net10.0-windows`, and tests exercise Win32, COM, UI, and native resource
  behavior. Do not model a Linux runner as an equivalent cheaper substitute.
- Preserve pull-request validation, required checks, test-result retention on
  failure, and least-privilege defaults. Cost proposals must identify which
  duplicate work they remove and which validation evidence remains.
- Use representative GitHub run durations and current billing rates before
  attaching savings numbers to a recommendation.
