# Citation catalog

The authority behind every [baseline.md](../baseline.md) choice. Each domain
table lists the **choice**, the **industry source** that backs it, and the
**rationale**. The closing [divergence notes](#divergence-notes) record where
reasonable, vetted setups legitimately differ - the places to "consult where
they collide" rather than treat as defects.

External links here are not network-checked by the offline link gate; keep them
correct by hand. Sources are canonical standards bodies and vendor docs, not
blog posts, so they age slowly.

## Umbrella frameworks

The cross-cutting standards the domain checks draw from.

| Framework | Source | What it anchors |
| --------- | ------ | --------------- |
| OpenSSF Scorecard | [scorecard checks](https://github.com/ossf/scorecard/blob/main/docs/checks.md) | The supply-chain checks and their risk levels (branch protection, pinned deps, token permissions, SAST, ...) |
| OpenSSF Best Practices Badge | [bestpractices.dev](https://www.bestpractices.dev/) | The passing/silver/gold criteria for OSS process health |
| OpenSSF Concise Guide | [secure-software guide](https://github.com/ossf/wg-best-practices-os-developers/blob/main/docs/Concise-Guide-for-Developing-More-Secure-Software.md) | A short, prioritized secure-development checklist |
| SLSA | [slsa.dev](https://slsa.dev/) | Build provenance and supply-chain integrity levels |
| OWASP Top Ten | [owasp.org/Top10](https://owasp.org/www-project-top-ten/) | The application-level vulnerability classes |
| Reproducible Builds | [reproducible-builds.org](https://reproducible-builds.org/) | Why deterministic, verifiable builds matter |
| .NET library guidance | [learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/) | The official "what and why" for high-quality .NET libraries |
| NuGet authoring best practices | [learn.microsoft.com](https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices) | Package metadata and packing recommendations |

## 1. Repository foundation and licensing

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| SPDX-identified open-source license | [SPDX license list](https://spdx.org/licenses/), [choosealicense.com](https://choosealicense.com/) | No license means exclusive copyright; an SPDX id is machine-readable on the gallery and by Scorecard |
| Pick the license deliberately | [GitHub: licensing a repository](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository) | The license governs all downstream use; default to MIT for permissive reuse |
| `.editorconfig` for style and severities | [EditorConfig](https://editorconfig.org/), [.NET code-style options](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options) | Makes style machine-enforceable instead of a review topic |
| `.gitattributes` line-ending normalization | [GitHub: configuring line endings](https://docs.github.com/en/get-started/getting-started-with-git/configuring-git-to-handle-line-endings) | Prevents cross-platform diff churn |
| Redistribution notices when bundling code | [REUSE](https://reuse.software/) | Satisfies attribution obligations of bundled licenses |

## 2. Build configuration

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| SDK-style projects | [NuGet: project format](https://learn.microsoft.com/en-us/nuget/resources/check-project-format) | The supported, metadata-in-project format |
| Central Package Management | [NuGet: central package management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) | One pinned version per package repo-wide; no inter-project drift |
| Lock file + restore audit | [Lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies), [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages) | Pins the full transitive graph (`--locked-mode` in CI) and flags advisories every restore |
| Pinned SDK in `global.json` | [global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) | Reproducible builds across machines and CI |
| Nullable reference types on | [Nullable references](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) | Opts into compiler null-safety |
| Analyzers on, warnings as errors | [.NET code analysis overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) | Stops warnings accumulating; turns analyzers on by default |
| Deterministic build / `ContinuousIntegrationBuild` | [.NET reproducible builds](https://github.com/dotnet/reproducible-builds), [MSBuild props](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#continuousintegrationbuild) | Path-independent, reproducible outputs and PDBs |
| Nullable reference types enabled repo-wide | [Nullable references](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) | Every project opts into compiler null-safety by default |
| AOT/trim-clean shippable project | [Prepare libraries for trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming), [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) | `IsAotCompatible` runs the trim/AOT analyzers so code composes into AOT apps and avoids runtime reflection |

## 3. Testing and coverage

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| A real test project and suite | [.NET unit-testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) | A repo without runnable tests cannot gate regressions; OpenSSF CI-Tests check |
| Microsoft Testing Platform (MTP) | [MTP overview](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro), [MTP + dotnet test](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-integration-dotnet-test) | The supported, self-contained successor to the VSTest host; the direction for all .NET test frameworks |
| Coverage collected and gated | [.NET code coverage](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage) | A patch-coverage gate holds new code to a bar without whole-project noise |
| Perf project where a hot path exists | [BenchmarkDotNet](https://benchmarkdotnet.org/) | Perf claims need measurement, not intuition |
| Fuzz untrusted-input surfaces | [OWASP: fuzzing](https://owasp.org/www-community/Fuzzing), [Scorecard: Fuzzing](https://github.com/ossf/scorecard/blob/main/docs/checks.md#fuzzing) | Coverage-guided fuzzing finds malformed-input bugs unit tests miss |

## 4. Packaging and publishing

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| Complete package metadata | [NuGet authoring best practices](https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices) | Metadata drives discoverability and trust |
| Source Link + symbols | [Source Link guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink), [symbol packages](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg) | Lets consumers step into library source while debugging |
| Package validation | [Package validation overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview) | Catches accidental breaking API and cross-framework gaps at pack time |
| README packed into the package | [PackageReadmeFile](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#packagereadmefile) | The README renders on the gallery package page |
| Strong naming for identity, applied consistently | [Strong naming guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/strong-naming) | Some consumers require it; never ship signed and unsigned variants |
| CLI tool packaged as a `dotnet` tool | [Create a dotnet tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create) | The supported distribution path for a command-line tool |

## 5. Versioning and releases

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| SemVer | [semver.org](https://semver.org/), [.NET versioning](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning) | Communicates compatibility expectations |
| Tag-driven deterministic versioning | [MinVer](https://github.com/adamralph/minver), [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) | The version is a function of the tag, not a hand-edit that drifts |
| A release record per version | [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), [GitHub releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) | Consumers need to know what changed and whether it breaks |
| Optional commit convention | [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) | Enables automated changelog and version inference |
| Build provenance / attestation | [GitHub artifact attestations](https://docs.github.com/en/actions/security-for-github-actions/using-artifact-attestations/using-artifact-attestations-to-establish-provenance-for-builds), [SLSA](https://slsa.dev/) | Lets consumers verify an artifact came from the expected build |

## 6. CI/CD

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| Harden the workflows | [GitHub: security hardening for Actions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions) | The canonical list of Actions-specific risks and mitigations |
| Least-privilege `GITHUB_TOKEN` | [Automatic token authentication](https://docs.github.com/en/actions/security-for-github-actions/security-guides/automatic-token-authentication), [Scorecard: Token-Permissions](https://github.com/ossf/scorecard/blob/main/docs/checks.md#token-permissions) | Limits blast radius if a step is compromised; default to `contents: read` |
| Pin actions by full commit SHA | [Scorecard: Pinned-Dependencies](https://github.com/ossf/scorecard/blob/main/docs/checks.md#pinned-dependencies), [StepSecurity](https://github.com/step-security/harden-runner) | A tag is mutable and can be repointed at malicious code; a SHA cannot |
| OIDC trusted publishing | [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), [GitHub OIDC](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect) | Removes the standing long-lived secret that is the usual leak |
| Concurrency and caching | [Concurrency](https://docs.github.com/en/actions/using-jobs/using-concurrency), [Caching](https://docs.github.com/en/actions/using-workflows/caching-dependencies-to-speed-up-workflows) | Cancels superseded runs and cuts CI time |
| Cost-aware topology and runner selection | [Actions runner pricing](https://docs.github.com/en/billing/reference/actions-runner-pricing), [Actions billing](https://docs.github.com/en/billing/concepts/product-billing/github-actions), [hosted-runner specifications](https://docs.github.com/en/actions/reference/runners/github-hosted-runners) | Each job has independent setup and billing granularity; current rates and visibility-specific hardware make runner and matrix choices materially different even when public-repository allowances hide invoice cost |
| Measure workflow and job usage | [Actions usage metrics](https://docs.github.com/en/enterprise-cloud@latest/organizations/collaborating-with-groups-in-organizations/viewing-usage-metrics-for-github-actions) | Job runtime, runner type, queue time, and failure rate distinguish actual hot spots from guesses |
| Manual extended validation | [Manually running workflows](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow) | Expensive breadth can remain available without charging every change, provided it has an explicit trigger and owner |
| Test the merge candidate | [Required status checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets#require-status-checks-to-pass-before-merging), [merge queues](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/configuring-pull-request-merges/managing-a-merge-queue) | Strict up-to-date checks or a merge queue prevent a green stale branch from merging as an untested combination |

## 7. Supply-chain and security

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| Branch protection / rulesets on the default branch | [GitHub: about rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets), [Scorecard: Branch-Protection](https://github.com/ossf/scorecard/blob/main/docs/checks.md#branch-protection) | Prevents direct, unreviewed, or history-rewriting changes to main |
| A dependency-update tool | [Dependabot version updates](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuring-dependabot-version-updates), [Renovate](https://docs.renovatebot.com/) | Out-of-date dependencies accrue known-vulnerable flaws |
| Single trusted feed (source mapping) | [Package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping) | Blocks dependency-confusion across configured feeds |
| Adopt only vetted, quarantined versions | [Renovate: minimumReleaseAge](https://docs.renovatebot.com/configuration-options/#minimumreleaseage), [OpenSSF Concise Guide](https://github.com/ossf/wg-best-practices-os-developers/blob/main/docs/Concise-Guide-for-Developing-More-Secure-Software.md) | A freshly published version is least-vetted and the usual compromised-release vector |
| Static analysis / code scanning | [CodeQL code scanning](https://docs.github.com/en/code-security/code-scanning/introduction-to-code-scanning/about-code-scanning-with-codeql), [code-scanning merge protection](https://docs.github.com/en/code-security/code-scanning/managing-your-code-scanning-configuration/set-code-scanning-merge-protection), [Scorecard: SAST](https://github.com/ossf/scorecard/blob/main/docs/checks.md#sast) | Catches injectable and memory-safety bug classes and blocks the merge while required analysis is absent, pending, or over threshold |
| Secret scanning + push protection | [About secret scanning](https://docs.github.com/en/code-security/secret-scanning/introduction/about-secret-scanning), [About push protection](https://docs.github.com/en/code-security/secret-scanning/introduction/about-push-protection) | Stops credentials from entering history |
| `SECURITY.md` with private reporting | [GitHub: adding a security policy](https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository), [Scorecard: Security-Policy](https://github.com/ossf/scorecard/blob/main/docs/checks.md#security-policy) | Gives reporters a safe path; a Scorecard signal |
| Dependency vulnerability auditing | [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages) | Surfaces advisories against the restored graph at build time |
| Review untrusted-input code against OWASP | [OWASP Top Ten](https://owasp.org/www-project-top-ten/) | The consensus application-vulnerability baseline |

## 8. OSS governance and community

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| Set up healthy-contribution files | [GitHub community standards](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions) | The community-profile checklist GitHub surfaces |
| `CODE_OF_CONDUCT.md` | [Contributor Covenant](https://www.contributor-covenant.org/) | Sets behavioral expectations; a community-standards signal |
| `CONTRIBUTING.md` | [Setting contribution guidelines](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors) | Lowers the barrier to a correct first contribution |
| `CODEOWNERS` | [About code owners](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners) | Routes review to the right owners automatically |
| Issue / PR templates | [About issue and PR templates](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/about-issue-and-pull-request-templates) | Makes reports and proposals actionable by default |
| `SUPPORT.md` | [Adding support resources](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project) | Directs help requests away from the issue tracker |

## 9. Agent enablement

| Choice | Source | Rationale |
| ------ | ------ | --------- |
| Vendor-neutral agent skills | [agentskills.io](https://agentskills.io/) | The discovered skills format across Copilot and Claude |
| `AGENTS.md` single source | [agents.md](https://agents.md/) | One canonical instruction set, many consumers |
| Generated tool mirror | [Copilot repository instructions](https://docs.github.com/en/copilot/customizing-copilot/adding-repository-custom-instructions-for-github-copilot), [VS Code customization](https://code.visualstudio.com/docs/copilot/copilot-customization) | Tool-specific files are mirrors of the canonical `AGENTS.md`, not hand-edited |

The mechanics of vendoring the skill tier and wiring the agent-file gates are
owned by the skill-lifecycle skill and the fleet onboarding runbook, which a
consuming repository names in its overlay.

## Divergence notes

Where vetted setups legitimately differ. Treat these as choices to record, not
defects to flag.

- **Action pinning: SHA vs major version.** The
  [Scorecard Pinned-Dependencies](https://github.com/ossf/scorecard/blob/main/docs/checks.md#pinned-dependencies)
  best practice is a full commit SHA (with the version in a trailing comment),
  and a dependency-update tool keeps the SHA fresh. Some repositories pin by
  major version instead, trading supply-chain strictness for lower maintenance
  and automatic patch uptake. The baseline recommends SHA pinning; a major-version
  policy is an accepted, stated divergence - record it rather than silently
  "fixing" it.
- **Changelog: a file vs GitHub Releases.** [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
  favors an in-repo `CHANGELOG.md`; tag-driven repos often curate
  [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases)
  notes instead. Either satisfies the baseline - pick one and state it in the
  README; do not require both.
- **Strong naming.** [Guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/strong-naming)
  treats it as identity, not a security boundary, and it is genuinely optional.
  The rule that is not optional: never ship both a signed and an unsigned variant
  of the same library.
- **Coverage thresholds.** A strict whole-project gate is noisy on small or
  fast-moving repos; a patch gate on changed lines is the pragmatic default. The
  baseline requires *a* gate, not a specific number.
- **Default-branch push validation.** A pull request with strict up-to-date
  checks or a merge-queue ruleset with no bypass path validates the merge
  candidate; repeating identical build/test work after merge is optional
  confirmation. Keep the push trigger when administrators, automation, or direct
  pushes can bypass equivalent pre-merge checks. Record the ruleset assumption
  next to the workflow decision.
- **Automatic matrix breadth.** Operating systems, architectures, target
  frameworks, Debug builds, and Native AOT legs belong on every merge only when
  they protect a merge-blocking invariant. It is reasonable to move lower-risk
  breadth to a scheduled/manual or release workflow, but document the resulting
  detection delay, cadence, and owner.
- **Branch protection as code vs UI.** Classic branch protection is configured in
  the UI; [repository rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets)
  can be exported and version-controlled as JSON. Prefer as-code for
  reproducibility, but UI-configured protection still satisfies the baseline.
