---
core: engineering-baseline
core-pin: v0.11.0
---

# Engineering baseline overlay

Repository-specific bindings for thirtytwo.

Assess the existing repository; do not run the greenfield scaffold over it.
Primary anchors are:

- [README.md](../../../README.md) and [LICENSE](../../../LICENSE);
- [Directory.Build.props](../../../Directory.Build.props) and
  [Directory.Packages.props](../../../Directory.Packages.props);
- [global.json](../../../global.json) and [nuget.config](../../../nuget.config);
- [thirtytwo.slnx](../../../thirtytwo.slnx);
- [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml).

The product is a Windows-only .NET 10 library and its tests exercise Win32,
COM, controls, and native resources. Keep platform-specific build and test
requirements explicit in any recommendation. Treat missing governance,
publishing, supply-chain, or agent-file gates as findings to discuss, not as
permission to add them automatically. Remote settings and GitHub changes still
require explicit user approval.
