---
core: performance-testing
core-pin: v0.17.0
---

# Performance testing overlay

Repository-specific bindings for thirtytwo.

- The perf project is
  [thirtytwo.perf](../../../thirtytwo.perf/thirtytwo.perf.csproj), targeting
  only `net10.0-windows`. Run the one affected supported TFM; do not invent a
  .NET Framework comparison.
- Build and run benchmarks from the repository root in Release. Keep generated
  reports and build trees under ignored `artifacts/`:

```pwsh
dotnet build --configuration Release thirtytwo.perf
dotnet run --configuration Release --framework net10.0-windows --project thirtytwo.perf -- --filter *<Name>* --artifacts artifacts/benchmarks
```

- Put each public benchmark class in its own file under `thirtytwo.perf`, in
  the `thirtytwo.perf` namespace. Follow the repository's XML documentation
  analyzer convention for every type.
- Use [scratch-buffer-strategy](../scratch-buffer-strategy/SKILL.md) to choose
  temporary storage and [il-copy-inspection](../il-copy-inspection/SKILL.md) to
  explain struct copies or boxing. Use
  [github-actions-cost-optimization](../github-actions-cost-optimization/SKILL.md)
  only for CI runner cost, not application runtime.
- No repository-specific source-line profiler is configured. Use the core's
  BenchmarkDotNet diagnostics and code-generation workflow where sufficient;
  otherwise report the missing profiler binding instead of inventing one.
