# Reproducible performance investigations

Detail for the [performance-testing](SKILL.md) skill. Use this workflow when a
performance question spans multiple phases, compares an external implementation,
iterates through several candidate optimizations, or must remain reproducible from
a dirty worktree.

For a simple A/B benchmark over one pure operation, use [authoring.md](authoring.md),
[running.md](running.md), and [interpreting-results.md](interpreting-results.md)
directly. Do not add phase harnesses or source bundles when the ordinary benchmark
already answers the question.

Use only the sections the task needs:

| Investigation shape | Read |
| --- | --- |
| Fresh-process CLI startup or multiple phases | [measurement contract](#establish-the-measurement-contract) |
| Multiple optimization candidates | [budget and stages](#bound-the-investigation-before-the-first-run) |
| Consumable/mutable phase inputs | [fresh-state measurement](#measure-phases-with-fresh-state) |
| External baseline or revision | [exact-source oracle](#compare-an-exact-source-oracle) |
| Dirty or generated retained inputs | [reconstructable run](#preserve-a-reconstructable-run) |

The expected outputs are a trustworthy benchmark or profile, a compact experiment
ledger, and enough source and run provenance to reconstruct any result that is kept.
Creating those local artifacts does not authorize committing, uploading, or
publishing them.

## Establish the measurement contract

Define the measured operation and its observable result before choosing a
harness. BenchmarkDotNet is a good fit for a repeatable in-process operation.
Use a purpose-built external harness when process startup, command discovery,
environment initialization, or a multi-phase CLI operation is part of the
question. Keep uninstrumented elapsed measurements separate from diagnostic
captures in either case.

Fail closed before accepting a run:

**Discovered work.** Require a nonzero discovered-case count, successful setup
with the expected subject identity, and one finite populated result for every
expected case. A zero exit code does not rescue zero cases, skipped setup, or
all-missing rows.

**Denominators.** Record the exact operation count and denominator for every
phase. Reconcile phase counts to the end-to-end operation population. When phases
cover different populations, report each denominator instead of adding or
comparing their totals.

**Semantics and artifacts.** Require equivalent nonempty semantic output or the
repository's explicit normalization. Retain the exact generated subject binary
and matching debug symbols used by the run; do not let a later build overwrite
them.

**Identity.** Record subject, analyzer, runtime, input/corpus, collector profile,
and relevant options. Pin the collector and requested sampling interval across
comparison arms, then verify interval semantics from the evidence before
converting CPU samples to time.

**Units.** Keep CPU samples or interval-derived CPU time, elapsed time, waits, and
GC pauses as separate measures. If interval provenance is absent or changes
incompatibly, report sample counts separately and mark the time comparison
`inconclusive`. Never estimate CPU time by multiplying wall time by a sampled CPU
share.

These are measurement-validity gates, not success criteria for the candidate. A
valid neutral or unfavorable result remains evidence and feeds the keep/reject/
inconclusive decision below.

### Name process and cache state

Use state labels that describe what the harness actually establishes:

| State | Required setup |
| --- | --- |
| Practical cold start | A fresh process plus a new isolated application/user home and application-owned cache state |
| Warm fresh-process | A fresh process using deliberately initialized application state from an earlier setup operation |
| Warm in-process | Repeated operations in one process with its managed and native state retained |

A fresh process clears that process's state; it does not flush operating-system
file caches, shared runtime state, kernel caches, or device caches. Do not call a
run cold merely because the process restarted. Record any cache state the harness
does not control instead of implying it was reset.

## Bound the investigation before the first run

Performance rigor should narrow a decision, not create an open-ended search. Before
editing, record these in the ledger:

- the user-facing outcome and numeric keep/reject gate;
- one small/common guardrail and one or two target scenarios;
- the cheapest check that can falsify the hypothesis;
- a maximum of three one-variable candidates unless the user approves more;
- a wall-clock budget (default 60-90 minutes when neither the user nor repository
  sets one), with a shorter budget for each stage;
- the escalation and stop rules below.

When the budget expires, preserve the evidence and report `inconclusive`; do not
silently turn the task into harness development, a larger matrix, or another round
of policy tuning. A missing reusable scenario or broken harness is separate
measurement work: fix or land it first, select a new baseline, and restart the
candidate investigation.

### Estimate product headroom before coding

Use the baseline profile or phase decomposition to estimate how much end-to-end
improvement the target can possibly deliver. If the target owns fraction $p$ of
product latency and the candidate speeds that phase up by factor $s$, Amdahl's law
gives the optimistic product speedup:

$$
S = \frac{1}{(1-p) + p/s}
$$

Treat the estimate as a screen, not a promise. If even eliminating the target phase
cannot reach the product gate, reject or choose a broader bottleneck before writing
production code. If the phase share is unknown, the first product pilot should
test the end-to-end effect cheaply rather than assuming a microbenchmark ratio will
survive startup, loading, I/O, rendering, or other pipeline work.

### Stage 1: screen the mechanism

Spend roughly 10-15 minutes on the smallest discriminating slice:

1. Run the narrow correctness/parity check.
2. Use a smoke or short benchmark job on one small guardrail, one representative
   target, and the largest useful target.
3. Change one material variable at a time and try no more than three candidates.
4. Reject immediately on semantic drift, a hard allocation/memory breach, a target
   regression, or a result too small to plausibly reach the predeclared product gate.

Do not broaden methods, input axes, target frameworks beyond those the production
surface requires, or policy combinations until one candidate survives this screen.
One local repair is reasonable when the result exposes a simple defect in the same
mechanism. A second redesign is a new candidate and consumes the candidate budget.
Changing scheduler/concurrency primitive, partition/merge shape, numeric reduction
order, cache state, or measurement affinity is a material variable, not a repair.
Treat floating-point reassociation and concurrent tie/order behavior as correctness:
pin exact totals/serialization or retain a sequential fallback before timing it.
Treat intuitive input axes such as size, depth, and cardinality as hypotheses, not
causes; vary them independently before claiming which axis controls the crossover.

### Stage 2: run a product pilot

Before a full matrix, test whether the isolated win survives the real product path:

- run the common guardrail and largest target, typically 3-5 launches per arm;
- compare exact output or the repository's explicit normalization;
- collect only the process timing and memory counters needed for the hard gate;
- predeclare a plausibility cutoff below the retained target (default 80% of that
  target, such as 8% for a 10% gate).

Before screening, choose a scenario-specific uncertainty rule and launch budget.
Every launch must succeed with equivalent output. Alternate or randomize paired
arms and retain their individual results; three to five launches are a coarse
screen, not a precise estimate of variance. A 5% CV limit can be useful for an
established stable scenario, but is not a universal gate for short processes.
Require a consistent direction and uncertainty small enough to distinguish the
predeclared plausibility cutoff. If the initial sample is insufficient, use the
predeclared additional launches without changing the candidate or measurement
conditions. If that budget cannot distinguish the effect, stop as `inconclusive`.
Reject a stable median improvement below the plausibility cutoff.

Reject a stable product regression or a result outside that plausibility margin.
Do not profile a candidate after it has failed a hard product gate. A lightweight
baseline profile may be needed to form the original hypothesis; the expensive
before/after attribution capture belongs after the product pilot passes, or when an
ambiguous pilot can be resolved by one specific target-frame query.

Keep three conclusions separate in the ledger and final report:

- **mechanism:** did the isolated operation improve, and at what allocation cost?;
- **product:** did the user-facing scenario improve within its latency/memory gate?;
- **attribution:** did the intended frame or phase shrink?

A mechanism win is not a product win. Attribution explains a candidate that is
otherwise eligible to ship; it cannot override a stable product regression or a
missed product threshold. Conversely, unchanged peak process memory does not waive a
per-operation allocation cap when that cap protects scalability or GC pressure.

### Stage 3: confirm the surviving candidate

Only a candidate that passes the first two stages earns broader confirmation:

- cover the affected operation families and meaningful input axes;
- accept or reject each operation family independently when throughput or allocation
  behavior differs; one shared mechanism does not imply one shared decision;
- use the full/default benchmark job and inspect complete error/allocation columns;
- run cold and warm forms when setup/cache state can change the answer;
- confirm the real product scenario at the repository's retained-run rigor and
  require the actual predeclared product gate, not the pilot's plausibility cutoff;
- validate every supported target framework and the repository's correctness gates.

Use a small control to expose fixed overhead, a target-scale case to exercise the
suspected cost, and a sustained case when the outcome is repeated throughput or
long-running latency. Vary scale rather than merely collecting more samples from
one tiny shape. A short best-case launch does not establish sustained speed; report
startup, steady operation, and any warmup or degradation separately when they
matter.

If two controlled reruns still miss the repository's noise/CV limit, stop as
`inconclusive`. Do not keep changing affinity, job shape, thresholds, or outlier
policy until one triplet happens to pass.

### Stage 4: retain evidence

Run the expensive evidence package only for a candidate that passed confirmation:
exact-source worktrees, alternating independent runs, high launch counts, full
telemetry, and before/after profile attribution. Profiling confirms where a retained
win came from; it does not override a failed product, correctness, allocation, or
memory gate.

Default hard stops for an optimization investigation are therefore:

- at most three policy candidates;
- at most one simple repair per candidate;
- at most two controlled noise reruns before `inconclusive`;
- no broad matrix before the mechanism screen passes;
- no retained run or attribution profile before the product pilot passes;
- no further optimization work after a hard gate fails, unless the user explicitly
  reopens the gate or approves a new investigation.

## Measure phases with fresh state

A mutable or consumable intermediate representation cannot be reused across
measurements. Reuse can make a later phase observe already-mutated state, shared
arrays, warmed caches, or an object graph that no longer represents a fresh
operation.

For phase latency:

1. Prepare a bounded batch of fresh inputs in `[IterationSetup]`.
2. Consume every item exactly once in the `[Benchmark]` method.
3. Set `OperationsPerInvoke` to the number of items consumed so BenchmarkDotNet
   reports per-operation time and allocation.
4. Release the batch in `[IterationCleanup]` so one iteration's live set does not
   leak into the next.
5. Run separate configurations for one item, an intermediate batch, and a larger
   batch. Keep `OperationsPerInvoke` accurate for each configuration.

The one-item run exposes timer and harness overhead. The larger run exposes
retained-live-set and GC distortion. Keep the smallest batch that amortizes the
harness without changing the workload's allocation or latency shape.

A consumable-state benchmark may need one invocation per iteration. BenchmarkDotNet
can then warn that the minimum iteration time was not reached. Document why the
invocation count must remain one; do not silence the warning by consuming the same
state repeatedly.

### Use a different harness for CPU profiling

The one-shot measurement harness is often too sparse for a useful periodic CPU
profile. Prefer an adaptive end-to-end benchmark that BenchmarkDotNet can repeat,
then use the repository's trace analyzer to scope the profile to the phase method.
Work before that phase call remains outside the selected subtree while the repeated
end-to-end operation supplies enough observations.

Profile the one-shot harness only when the selected phase query has enough
contributing records under the analyzer's sample-quality contract. A whole-trace
sample count does not establish quality for a narrow phase.

Before trusting a decomposition, compare it with an independently measured
end-to-end operation:

- phase allocations should add to the end-to-end allocation at the report's
  precision;
- phase means should approximately add to the end-to-end mean;
- each phase should receive fresh, semantically equivalent state.

A large gap usually means setup leaked into a measurement, state was reused, or
the batch changed GC and live-set behavior.

## Keep an experiment ledger

Start the ledger before the first edit and add one row per candidate. Record
rejected variants as carefully as retained ones.

| Hypothesis | Small edit | Discriminating check | Time | Allocation | Target frame | Decision / evidence gap |
| --- | --- | --- | ---: | ---: | --- | --- |
| Example claim | One-variable change | Same filtered benchmark | Result | Result | Before -> after | Keep, reject, or inconclusive; name the missing evidence |

Change one material variable at a time. Use the same scenario, filter, target
framework, job, and profiler scope before and after. Preserve both target
frameworks when the production code serves both. A rejection is useful evidence:
it prevents a later investigation from retrying an attractive idea that already
lost on throughput, allocation, another runtime, or the intended target frame.

The final report should explain why the retained implementation beat at least the
most plausible alternative, not merely state that the retained row was faster than
the original baseline.

## Decide, stop, and resume

Use the narrowest state supported by the evidence:

| State | Meaning |
| --- | --- |
| Keep | Semantic gates pass and the candidate meets the predeclared product and resource gates |
| Reject | A valid result is neutral, unfavorable, or below the keep threshold, or a semantic/resource gate fails |
| Inconclusive | The evidence is noisy, incompatible, incomplete, or invalid for the claimed comparison |
| Evidence-blocked | A named prerequisite such as symbols, operation counts, identity, or a working harness is missing |
| Diminishing returns | Valid coverage remains, but further eligible hypotheses have less plausible headroom than the remaining budget justifies |

Do not relabel missing evidence as diminishing returns. Preserve neutral and
rejected candidates; they prevent repeated dead ends. A compact resume record names
the next hypothesis, completed and invalid attempts, exact identities and artifact
locations, semantic and measurement gates, remaining evidence gaps, and the current
budget/stop decision. Publication state is separate from investigation progress.

## Compare an exact-source oracle

When the baseline lives in another repository or revision, compare against exact
source rather than an unpinned package or a mutable checkout:

1. Create a clean detached checkout at an exact commit SHA.
2. Build it in Release with the subject repository's pinned SDK. Record the SDK
   version and any compatibility override.
3. Verify the built assembly's informational version and configuration when that
   metadata is available, and retain its SHA-256 hash.
4. Isolate namespace and type collisions with an `extern alias` rather than
   renaming either implementation.
5. Make the oracle reference opt-in. Set an environment variable whose name is
   also an optional MSBuild property before launching BenchmarkDotNet; the
   generated child build inherits the environment, while an outer `/p:` argument
   alone may not reach that child build.
6. Include the oracle reference and benchmark methods only when that property is
   present. An ordinary build must contain neither.
7. Validate semantic parity before measuring: equivalent inputs, outputs,
   exceptions, options, and fresh mutable state for every operation.
8. Remove the temporary checkout after the retained artifacts record its commit
   and assembly hash.

Do not assume a parsed model or decoded object graph can be reused safely. Prove
that repeated materialization is independent, or reconstruct the intermediate
state per operation. Shared mutable arrays or caches can make an oracle appear
faster while changing the work being measured.

As a build check, inspect the generated BenchmarkDotNet child project for an
opt-in run and confirm that it contains the exact oracle reference. Then build
without the environment property and confirm that no oracle reference or
oracle-only benchmark method remains.

## Preserve a reconstructable run

Give every retained run a unique directory or output stem, for example:

```text
<subject>-<phase>-<variant>-<tfm>-<job>-<timestamp>
```

Retain the compact report and raw result, exact command line, non-secret
environment settings, runtime and JIT identity, OS and architecture, base commit,
clean/dirty state, experiment-ledger row, and any trace manifest. Keep separate
run directories so a later BenchmarkDotNet invocation cannot overwrite the
baseline used in a claim.

### Dirty-source bundle contract

A commit SHA plus hashes is insufficient when tracked, untracked, binary, or
ignored inputs affected the run. For a dirty worktree, retain one of these:

- a complete source snapshot; or
- a base commit plus all of the following reconstruction artifacts.

The reconstruction artifacts are:

1. A binary-capable full-index patch of every tracked difference from the recorded
   base commit, including staged and unstaged changes. Store that commit in
   `$baseCommit`, then use `git diff --binary --full-index $baseCommit --` so the
   patch and restore point cannot disagree.
2. An archive containing the bytes of every relevant untracked or ignored source,
   generated input, and benchmark data file, stored at its repository-relative
   path. Build the archive from an explicit allowlist after reviewing each file
   for credentials, signing material, personal data, proprietary inputs, and
   unrelated local configuration. Include required file metadata when it affects
   the build or run.
3. A manifest containing the base commit, repository-relative path, tracked /
   untracked / ignored classification, byte length, and SHA-256 hash for every
   archived input, plus hashes of the patch and archive themselves. Record who or
   what performed the allowlist review and list any non-secret external
   provisioning requirements.
4. The exact restore procedure: detach the base commit, apply the binary patch,
   extract the archive at the repository root, and verify the manifest hashes.

Hashes prove integrity; they do not replace missing content. Never archive a file
merely because it is ignored or untracked. Do not archive credentials, signing
material, package caches, unrelated build output, or data whose redistribution is
not authorized. When a secret influences setup, record a non-secret provisioning
requirement rather than the secret. If the run cannot be reconstructed without
retaining sensitive bytes, do not publish or share the bundle; keep the result
local or redesign the input so a safe equivalent can be retained.

For a result that will support a durable claim, test the restore procedure in a
temporary clean checkout before discarding the original worktree. The restored
checkout must reproduce the recorded source hashes and include every build or
benchmark input that was not obtainable from the base commit.

## Acceptance checks

The workflow is complete when all applicable checks pass:

- A consumable parse/materialize split uses fresh-state batches for measurement
  and an adaptive end-to-end path for profiling unless the one-shot phase has
  enough query-level evidence.
- Every accepted run has nonzero discovered work, populated expected rows, reconciled operation/phase denominators, equivalent semantic output, and exact subject/debug identities.
- CPU samples, interval-derived CPU time, elapsed time, waits, and GC pauses retain distinct units; incompatible or absent interval provenance cannot produce a time comparison.
- The ledger preserves rejected variants and explains the final decision.
- An opt-in BenchmarkDotNet child build references the assembly built from the
  recorded oracle commit and hash, semantic-parity checks pass on fresh state, and
  an ordinary build has no oracle surface.
- A clean checkout plus the retained dirty-source bundle reconstructs and verifies
  every source and input byte needed for the run; its explicit allowlist contains
  no secret, unauthorized, or unrelated content.
- The retained command, non-secret environment, runtime/JIT, OS/architecture,
  target framework, job, and profiler scope identify the execution environment
  closely enough to rerun the same experiment.
