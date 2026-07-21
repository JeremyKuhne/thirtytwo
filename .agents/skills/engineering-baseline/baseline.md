# The baseline

The normative gold standard, grouped into nine domains. Each item states the
**recommended choice** and a one-line **why**; the external authority and the
divergence notes are in the [citation catalog](references/best-practices.md),
keyed by the same domain numbers.

Each item is tagged:

- **(core)** - every repository should satisfy it.
- **(library)** - applies when the repo ships a NuGet package.
- **(tool)** - applies when the repo ships an executable / `dotnet` tool.
- **(conditional: <condition>)** - applies only when the named condition holds;
  the parenthetical states the trigger so applicability is mechanical (for
  example *(conditional: untrusted input)*).
- **(deferred)** - in scope, but the actual judgment is owned by another skill;
  confirm the check is wired, do not perform it here.

Scoring and remediation read these tags to decide applicability; see
[assess.md](assess.md). An item marked **temporal** applies only after a
milestone ("once a stable release exists"); before that milestone it scores
**N/A** with a note, not **Missing**. Concrete file names below
(`Directory.Build.props`, `global.json`, ...) are .NET ecosystem conventions,
not repo-specific paths. A repository's own layout, target frameworks, and
project names are bound in its overlay - this core states the *shape*, not the
literals.

## 1. Repository foundation and licensing

- **(core)** A top-level `LICENSE` file with an [SPDX](https://spdx.org/licenses/)-identified
  open-source license (MIT is the fleet default). Why: without a license the
  project defaults to exclusive copyright and cannot be used or reviewed.
- **(core)** A `README.md` that states what the project is, how to install it,
  and where to find docs. Why: it is the first and most-read artifact, and it
  doubles as the package landing page.
- **(core)** A `.gitignore` scoped to the toolchain (build output, `obj/`,
  `bin/`, IDE state) and a `.gitattributes` that normalizes line endings
  (`* text=auto`). Why: keeps generated and machine-specific files out of history
  and avoids cross-platform diff churn.
- **(core)** An `.editorconfig` that sets indentation, encoding, naming rules,
  and analyzer severities. Why: makes style machine-enforceable instead of a
  review topic.
- **(core)** A license header on every source file, enforced
  (`file_header_template` + `IDE0073`). Why: each file states its license and
  copyright with a machine-readable SPDX id. Omit a per-file year (maintenance
  churn, no legal benefit) and credit "<owner> and contributors" rather than a
  roster.
- **(conditional: redistributes third-party code)** A `NOTICE` /
  `THIRD-PARTY-NOTICES` file when third-party code is redistributed. Why:
  satisfies attribution obligations of bundled licenses.

## 2. Build configuration

- **(core)** SDK-style projects; framework and package metadata in the project
  file. Why: the supported, tooling-friendly project format.
- **(core)** Central build properties in `Directory.Build.props` /
  `Directory.Build.targets`. Why: one source of truth for language version,
  nullability, and analysis settings across every project.
- **(core)** Route build output and intermediates to a top-level `artifacts/`
  tree (for example `artifacts/bin/` and `artifacts/obj/` via
  `BaseOutputPath` / `BaseIntermediateOutputPath`). Why: keeps repository roots
  clean, makes cleanup deterministic, and standardizes paths for CI artifacts.
- **(core)** Central Package Management via `Directory.Packages.props`
  (`ManagePackageVersionsCentrally=true`). Why: one pinned version per package,
  repo-wide, removing version drift between projects.
- **(core)** Commit a lock file (`packages.lock.json` via
  `RestorePackagesWithLockFile`) and enable NuGet audit (`NuGetAudit` with
  `NuGetAuditMode=all`); CI restores with `--locked-mode`. Why: pins the full
  dependency graph including transitives, fails on drift, and flags advisories on
  direct and transitive packages every restore.
- **(core)** A pinned SDK in `global.json` with a `rollForward` policy. Why:
  reproducible builds across machines and CI; no silent SDK drift.
- **(core)** `Nullable=enable` globally (in `Directory.Build.props`), plus a
  current `LangVersion` and a deliberate `ImplicitUsings` setting. Why: every
  project opts into the compiler's null-safety analysis by default.
- **(core)** Keep the shippable project AOT/trim clean on its modern target
  (`IsAotCompatible=true`), which turns on the trim, AOT, and single-file
  analyzers. Why: the code composes into trimmed/AOT apps and avoids expensive
  runtime reflection - worth keeping clean even if you never publish AOT, and
  with warnings-as-errors a regression fails the build.
- **(core)** `TreatWarningsAsErrors=true` (with a short, documented
  `WarningsNotAsErrors` escape list) and `EnableNETAnalyzers` with a chosen
  `AnalysisLevel` / `AnalysisMode`. Why: keeps warnings from accumulating and
  turns the analyzers on by default.
- **(core)** `EnforceCodeStyleInBuild=true`. Why: surfaces the `.editorconfig`
  IDE rules as build diagnostics so style is enforced headlessly in CI.
- **(library)** `GenerateDocumentationFile=true`. Why: ships XML docs and flags
  undocumented public surface.
- **(core)** Deterministic build, with `ContinuousIntegrationBuild=true` set
  under the CI environment. Why: reproducible, path-independent outputs and PDBs.

## 3. Testing and coverage

- **(core)** A dedicated test project on the
  [Microsoft Testing Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro)
  (MTP): the test project is an executable (`OutputType=Exe`) with an MTP runner
  (`EnableMSTestRunner` for MSTest, `UseMicrosoftTestingPlatformRunner` for
  xunit.v3), selected in `global.json` (`"test": { "runner":
  "Microsoft.Testing.Platform" }`). Why: MTP is the supported, self-contained,
  faster successor to the VSTest host and the direction for every .NET test
  framework.
- **(core)** Default to MSTest; when using xunit, use v3 - both are first-class
  on MTP. Why: a current, MTP-native framework; the fleet defaults away from
  xunit's contribution policy on AI-assisted changes. This is a fleet choice, not
  a hard rule - an overlay may pick another MTP-capable framework.
- **(core)** Run at the fullest concurrency the framework allows by default
  (MSTest `[assembly: Parallelize(Workers = 0, Scope = MethodLevel)]`; xunit.v3
  unlimited parallel threads). Why: fast feedback, and it surfaces hidden test
  ordering or shared-state bugs early.
- **(core)** Tests run in Release on every merge candidate. Run them again on a
  default-branch push only when a direct or bypass update can avoid equivalent
  pre-merge validation; a pull request with strict up-to-date checks or a merge
  queue should not pay for an identical post-merge run. Why: Release codegen and
  timing differ from Debug; gate what ships without duplicating the same
  candidate.
- **(core)** Code coverage collected and reported, with a gate on patch
  coverage (new/changed lines). Why: a patch gate holds new code to a bar
  without fighting noise in the whole-project number.
- **(conditional: perf claim or hot path)** A perf project (for example
  BenchmarkDotNet) where a performance claim or hot path exists - a hot path is
  code on a measured critical path or tight loop (parsing, encoding, crypto,
  byte/image processing), or any explicit performance claim in the README. Why:
  perf assertions need measurement, not intuition.
- **(conditional: untrusted input)** A fuzz project (for example SharpFuzz) for
  any parser, decoder, or buffer surface that takes untrusted input. Why:
  coverage-guided fuzzing finds the malformed-input bugs unit tests miss.

## 4. Packaging and publishing

- **(library)** Complete package metadata: `PackageId`, `Description`,
  `Authors`, `PackageLicenseExpression`, `PackageProjectUrl`, `RepositoryUrl`,
  `RepositoryType`, `PackageReadmeFile`, and `PackageTags`. Why: metadata drives
  discoverability and trust on the gallery.
- **(library)** [Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)
  enabled (`PublishRepositoryUrl`, the Source Link package, `EmbedUntrackedSources`)
  with symbols published (embedded PDBs or a `.snupkg`). Why: lets consumers
  step into library source while debugging.
- **(library, temporal)** Package validation (`EnablePackageValidation`, with a
  baseline version once a stable release exists). Temporal: score N/A until the
  first stable release, then required. Why: catches accidental breaking API and
  cross-framework surface gaps at pack time.
- **(library)** A README packed into the package (`PackageReadmeFile`). Why: the
  README renders on the gallery package page.
- **(library)** Strong-named assemblies for identity (not as a security
  boundary), shipped consistently. Why: required by some consumers; do not ship
  both signed and unsigned variants of the same library.
- **(tool)** `PackAsTool=true` with a `ToolCommandName`, packaged for
  `dotnet tool` install. Why: the supported distribution path for a CLI tool.

## 5. Versioning and releases

- **(core)** [SemVer](https://semver.org/) version numbers. Why: communicates
  compatibility expectations to consumers.
- **(library)** Tag-driven, deterministic versioning from git history
  (for example MinVer or Nerdbank.GitVersioning) rather than a hand-edited
  version. Why: the released version is a function of the tag, not a manual edit
  that can drift.
- **(core)** A release record per version - a `CHANGELOG.md`
  ([Keep a Changelog](https://keepachangelog.com/en/1.1.0/)) or curated GitHub
  Releases notes. Why: consumers need to know what changed and whether it breaks.
- **(library)** Releases are immutable once published, and the package is
  published only from a tagged commit. Why: a pinned version must never change
  underneath a consumer.
- **(library)** Releases are signed or carry build provenance / attestation.
  Why: lets consumers verify an artifact came from the expected build.

## 6. CI/CD

- **(core)** A build-and-test workflow triggered for every merge candidate and
  available for manual diagnosis. Add a default-branch push trigger only when
  branch rules permit an update that bypasses equivalent pre-merge validation;
  include `merge_group` when a merge queue is available. Why: gate every path to
  the default branch without automatically testing the same change twice.
- **(core)** A least-privilege token: top-level `permissions: contents: read`,
  with any write scope granted per-job. Why: limits blast radius if a workflow
  step is compromised.
- **(core)** Third-party actions pinned by full commit SHA (with the version in
  a trailing comment). Why: a tag or branch is mutable and can be repointed at
  malicious code; a SHA cannot.
- **(core)** Concurrency control that cancels superseded in-flight runs for a
  pull request. Why: saves runner time and surfaces only the latest result.
- **(core)** The automatic matrix contains the minimum load-bearing Release
  gates; Debug and expensive platform/architecture breadth run through a named
  manual, scheduled, or release workflow with a named owner and occasion or
  cadence when delayed detection is acceptable. Why: preserve the support
  contract without charging every pull request for every dimension.
- **(core)** Each job uses the least expensive runner that is proven compatible;
  native behavior still executes on its required operating system and
  architecture. Why: lightweight scripts do not need premium runners, while
  cross-compilation does not replace native execution.
- **(conditional: restorable dependencies)** Dependency caching keyed on the
  lock/props files and pointed at the directory the tool actually uses. Why:
  cuts CI time without staleness or a cache that never captures restored data.
- **(core)** A stable aggregate status-check name that branch protection can
  require, even when the matrix underneath changes. Why: keeps the required
  check name from breaking when the build matrix is edited.
- **(library)** Publishing via OIDC
  [trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
  (short-lived key, `id-token: write`) rather than a stored long-lived API key.
  Why: removes the standing secret that is the usual supply-chain leak.

## 7. Supply-chain and security

- **(core)** A `SECURITY.md` with a private reporting channel and a disclosure
  expectation. Why: gives reporters a safe path and is an
  [OpenSSF Scorecard](https://github.com/ossf/scorecard/blob/main/docs/checks.md)
  Security-Policy signal.
- **(core)** A dependency-update tool (Dependabot or Renovate) configured. Why:
  out-of-date dependencies accrue known-vulnerable flaws; automate the bumps.
- **(core)** Restore from a single trusted feed with package source mapping (a
  `nuget.config` that clears inherited sources and maps every package to one
  feed). Why: blocks dependency-confusion, where a package is silently served
  from an unexpected source.
- **(core)** Adopt only vetted, quarantined dependency versions - pin a
  known-good floor and bump deliberately (a minimum release age, advisory,
  deprecation, and listing checks, and a license allowlist) rather than tracking
  latest. Why: a freshly published version is the least-vetted and the usual
  vector for a compromised release.
- **(core)** Static analysis / code scanning (for example CodeQL) on pull
  requests (and `merge_group` when used) and on a schedule, with code-scanning
  results required by merge protection at documented thresholds. Moving it to
  schedule-only is an accepted divergence only after documenting the
  delayed-detection security tradeoff, cadence, and alert owner. Why: catches
  injectable and memory-safety bug classes before merge by default.
- **(core)** Secret scanning and push protection enabled. Why: stops credentials
  from entering history.
- **(core)** Branch protection or a repository ruleset on the default branch:
  require the status and code-scanning checks, require pull requests with no
  bypass, require branches up to date or a merge queue, and block force-push and
  deletion. Why: prevents direct, unreviewed, stale-base, or history-rewriting
  changes to main.
- **(conditional: untrusted input) (deferred)** Address the
  [OWASP Top Ten](https://owasp.org/www-project-top-ten/) classes for any code
  that handles untrusted input or runs as a service - the security-review skill
  owns the actual review; here, only confirm it has been run. Why: the consensus
  baseline for application-level vulnerability review.

## 8. OSS governance and community

- **(core)** A `CODE_OF_CONDUCT.md` (the
  [Contributor Covenant](https://www.contributor-covenant.org/) is the default).
  Why: sets behavioral expectations and is a GitHub community-standards signal.
- **(core)** A `CONTRIBUTING.md` covering how to build, test, and submit
  changes, and the contribution license terms. Why: lowers the barrier to a
  correct first contribution.
- **(conditional: multiple maintainers)** A `CODEOWNERS` file once more than one
  maintainer or area exists. Why: routes review to the right owners automatically.
- **(core)** Issue and pull-request templates. Why: makes reports and proposals
  actionable by default.
- **(conditional: defined support channel)** A `SUPPORT.md` when there is a
  defined support channel. Why: directs help requests away from the issue tracker.

## 9. Agent enablement

- **(core)** An `AGENTS.md` as the single source of agent guidance, with the
  tool-specific mirror (for example `.github/copilot-instructions.md`) generated
  from it, not hand-edited. Why: one canonical instruction set, many consumers.
- **(core)** Vendored agent skills under the host-read skills location, each
  pinned and provenance-stamped, with the repo-specific overlay. Why: the
  reviewed, version-controlled skill set instead of ad-hoc prompting.
- **(core)** An agent-file gate in CI: frontmatter validation, the mirror check,
  markdown lint, and an offline link check over the agent files. Why: keeps the
  instruction set valid and its internal links resolvable.
- **(conditional: distinct domains)** Path-specific instructions and reviewer
  agent personas where the repo has distinct domains. Why: focuses guidance and
  review on the area being changed.

The mechanics of *vendoring* the skill tier, wiring the validator and link
checker, and the drift loop are owned by the skill-lifecycle skill and the fleet
onboarding runbook; this domain confirms they are present, and defers the how to
them. A consuming repository names both in its overlay.
