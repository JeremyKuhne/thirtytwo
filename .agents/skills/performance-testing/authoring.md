# Authoring a benchmark

Detail for the [performance-testing](SKILL.md) skill. Covers file/class layout,
the globals already imported, the required and optional attributes, and what a
benchmark method must do.

## File and class layout

- One benchmark class per file, file named after the class (e.g. `MyBenchmark.cs`).
- Namespace is the perf project's namespace (`<root>.perf` by convention).
- Class is `public` (BenchmarkDotNet requires this) and contains one or more
  methods marked `[Benchmark]`.
- Use the repo's standard file header.
- Follow the repo coding style (the conventions in its `AGENTS.md` /
  contributor guide - type-name style, null-check style, XML-doc indentation,
  etc.).

## Globals already imported

A perf project usually centralizes common usings in a `GlobalUsings.cs`. Typical
contents:

- `BenchmarkDotNet.Attributes` - `[Benchmark]`, `[MemoryDiagnoser]`, `[Params]`,
  `[GlobalSetup]`, etc.
- `BenchmarkDotNet.Jobs` - `RuntimeMoniker`, `[SimpleJob]`.
- The library root namespace.
- Any per-TFM IO/compat namespaces the repo standardizes on.

Check the perf project's `GlobalUsings.cs` and do not re-import what it already
provides.

## Required attributes for memory evaluation

Always annotate every benchmark class with `[MemoryDiagnoser]`, regardless of its
purpose. This adds three columns to the results table: **Gen0 / Gen1 / Gen2**
(collections per 1000 ops) and **Allocated** (bytes per op). Without it you only
get timings.

```c#
[MemoryDiagnoser]
public class StringFormatting
{
    [Benchmark(Baseline = true)]
    public string StringFormat() => string.Format("The answer is {0}.", _value);

    [Benchmark]
    public string CustomFormat() => MyFormatter.Format("The answer is {0}.", _value);
}
```

Mark one method `[Benchmark(Baseline = true)]` whenever you are comparing
alternative implementations - the report adds a **Ratio** and **RatioSD** column
relative to that baseline.

## Optional attributes

- `[SimpleJob(RuntimeMoniker.HostProcess, warmupCount: 1, iterationCount: 3, launchCount: 1)]`
  - cuts run time when iterating on a benchmark. Use only while developing;
  remove (or revert to defaults) before checking in stable measurements.
- `[Params(...)]` on a public field/property to run the benchmark for each value.
- `[GlobalSetup]` for one-time setup outside the measured region.
- `[Arguments(...)]` to pass per-method parameters.

## What a benchmark method must do

- **Make the measured work observable.** Prefer returning a value derived from
  the work. For a `void`-returning API, consume changed state or validate it
  outside the timed region; use BenchmarkDotNet's consumer when no natural
  result exists. Do not return a constant, `buffer.Length`, or another value
  invariant of execution. For buffer-mutating APIs (`Span<T>.Replace`,
  `Random.NextBytes`, `Encoding.GetBytes`), return an element or a digest whose
  value depends on the mutation. A `void` benchmark is not automatically
  eliminated, but pure work whose result is unobserved can be. Near-zero or
  unstable timings are a reason to inspect generated code and semantic output,
  not proof of one specific optimizer action. See
  [BenchmarkDotNet good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html).
- Be cheap to call repeatedly - BenchmarkDotNet invokes it millions of times.
- Avoid per-call setup; move setup into `[GlobalSetup]` or readonly fields.
- Call the system-under-test directly where practical. Resolve overloads with
  explicitly typed fields, casts, or separate benchmark classes. If a wrapper is
  unavoidable, keep the same wrapper shape in every comparison arm and account
  for its overhead; do not rename a product API merely to make a benchmark bind.
- Ref structs cannot be returned from `[Benchmark]` methods; consume them inside
  the method and return a representative scalar (length or hash).
