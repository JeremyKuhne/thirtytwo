# Interpreting results

Detail for the [performance-testing](SKILL.md) skill. Covers the before/after
measurement discipline and reading the memory columns.

## Before/after measurement discipline

A performance claim is not supported until semantic equivalence and the
predeclared latency, allocation, and memory guardrails pass.
Profiling a trace answers *where* to optimize; only a before/after benchmark
answers *whether it worked*. Treat the following as mandatory whenever a code
change is driven by a profile:

- **Capture a baseline first on every affected supported TFM.** Run the affected
  benchmark on each production target *before* touching the source and save the
  full result rows.
  The line-level EventPipe profiling that guides the edit is modern-runtime-only,
  but a repository that also supports .NET Framework must measure that target
  directly because its JIT and BCL allocate and inline differently.
- **Re-run every affected supported TFM after the change** and diff against the
  saved baseline.
- **Record the full BenchmarkDotNet rows, not just a one-line summary.** Keep the
  whole table - `Mean`, `Error`, `StdDev`, `Gen0`, `Gen1`, `Gen2`, `Allocated` -
  for both the before and after runs on every measured TFM. A summary like "~7% faster"
  loses the error bars (is the delta inside the noise?) and the allocation column
  (did the speedup trade CPU for garbage?). Save the raw output to
  `artifacts/<name>-baseline-<tfm>.txt` and `artifacts/<name>-after-<tfm>.txt`
  so the full diff is reproducible. Pipe the run through `Tee-Object` to keep the
  console table.
- **Apply the predeclared allocation gate.** Compare the `Allocated` column
  before and after on every measured TFM. "No additional allocations" is only
  true if all applicable columns are unchanged.
- **Confirm the targeted cost actually moved.** Re-capture a trace after the
  change and check that the specific method/line the heat map flagged dropped
  (e.g. `System.Array.Copy` self-time falling from 30 ms to 19 ms). A faster
  wall-clock with the targeted frame unchanged usually means the win came from
  somewhere else - or from noise.
- **Distrust sub-microsecond deltas from a noisy machine.** A busy or thermally
  throttled host shows variance swings; trust the `Allocated` column and a unit
  test over a small `Mean` delta. Deltas comfortably outside `Error`/`StdDev` and
  consistent across repeated runs and affected TFMs have stronger support.

Present the supported TFMs' before/after tables together when reporting the
result, plus the line-level evidence that the targeted hot spot shrank.

## Evaluating memory usage

First identify which memory question the user asked. These measures are not
substitutes for one another:

| Measure | What it supports |
| --- | --- |
| BenchmarkDotNet `Allocated` | cumulative managed bytes allocated per operation |
| Sampled allocation profile | estimated allocation types/sites under that profiler's sampling contract |
| Collection counts and GC pauses | collection frequency and pause cost, separate from allocation volume |
| Heap/liveness measurement at a named boundary | live or retained bytes and ownership at that boundary |
| Process working set or peak memory | process-level committed/resident pressure, including costs outside the managed heap |

Do not call sampled allocation cumulative bytes unless the profiler establishes
that conversion. Do not infer retained or output-owned memory by subtracting file
size, final heap size, or result size from cumulative allocation. Establish
result, intermediate, and cache ownership at a named lifetime boundary with a
heap/liveness tool or explicit object ownership evidence.

With `[MemoryDiagnoser]` (or `--memory`), each row of the results table includes:

| Column      | Meaning                                                      |
| ----------- | ------------------------------------------------------------ |
| `Gen0`      | Gen0 collections per 1000 operations.                        |
| `Gen1`      | Gen1 collections per 1000 operations.                        |
| `Gen2`      | Gen2 collections per 1000 operations.                        |
| `Allocated` | Managed bytes allocated per single operation.                |

### Reading the numbers

- **`Allocated` is the primary signal.** A method that should be allocation-free
  must report `-` or `0 B`. Anything else is a regression.
- A `Ratio` column appears when one method is `Baseline = true`. Use it together
  with `Allocated` to confirm a perf change is not just trading CPU for
  allocations (or vice versa).
- `Gen0` ticking up while `Allocated` stays flat usually means you allocated a
  lot of short-lived objects on previous iterations - recheck `[GlobalSetup]`.
- Stack-only types (`Span<T>`, `ref struct`s, `stackalloc`) do not contribute to
  `Allocated`. Boxing of value types does - watch for accidental boxing through
  `object`, non-generic interfaces, or `string.Format`.
- When a repository supports .NET Framework and a modern target, measure both;
  their JITs and BCLs can allocate differently.

### Where the report lives

After each run BenchmarkDotNet writes artifacts under
`BenchmarkDotNet.Artifacts/results/` next to the executable, including:

- `*.md` - GitHub-friendly Markdown table (paste this into PR descriptions).
- `*.csv` and `*.html` - for spreadsheets / browser viewing.
- `*-report-full.json` - raw measurements; useful for diffing two runs
  programmatically.

The same table is also printed to the console at the end of the run.
