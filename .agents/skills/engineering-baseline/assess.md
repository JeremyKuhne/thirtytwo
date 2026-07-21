# Assess an existing repository

The brownfield verb: measure a repository against [baseline.md](baseline.md),
report the gaps by risk, and converge - applying safe fixes directly and
proposing the rest. Triggered by "ensure this repo follows modern engineering
best practices", "audit this repository", or "bring this repo up to standard".

The output is a **scored gap report plus an applied set of safe fixes**, never a
silent rewrite. Stop at the remote boundary (see the core's guardrails) and at
anything that needs a judgment call.

## 1. Inventory

Read the repository's actual state before judging it. Gather, read-only:

- Root files: `LICENSE`, `README.md`, `SECURITY.md`, `CONTRIBUTING.md`,
  `CODE_OF_CONDUCT.md`, `CHANGELOG.md`, `.editorconfig`, `.gitignore`,
  `.gitattributes`, `global.json`, `Directory.Build.props` / `.targets`,
  `Directory.Packages.props`.
- The project graph: which projects exist, their target frameworks, which is the
  shippable one, whether test / perf / fuzz / analyzer projects are present.
- `.github/`: workflows, `dependabot.yml`, `CODEOWNERS`, issue/PR templates,
  rulesets, `copilot-instructions.md`.
- CI topology: triggers, matrices, runners, required status names, repeated
  setup, caches/artifacts, and whether one merged change causes duplicate runs.
- Agent wiring: `AGENTS.md`, the skills location, the agent-file validator and
  its CI gate.
- Remote settings you can read without admin: read them from the repository's
  own files rather than the GitHub API - the workflow YAML shows whether publish
  uses OIDC (`id-token: write` plus a trusted-publishing login step) or a stored
  key. Toggles that need an admin token (branch protection / rulesets, secret
  scanning, push protection) cannot be read from the tree; mark them
  **unverified - admin required** rather than guessing.

Record two things that drive applicability: the primary **archetype** (tool,
library, multi-target library, or app), which selects the archetype tags, and
the **release status** (pre-release, stable, or end-of-life). Determine release
status from the tag list (read-only `git tag` or `.git/refs/tags`) and the
package gallery; if there are no tags and no published package, treat the repo as
**pre-release**, corroborated by README markers like "coming soon". Release
status decides whether the temporal items (package validation, release record,
immutability, signing) apply yet or score N/A.

## 2. Score by domain

For each of the nine [baseline.md](baseline.md) domains, mark every applicable
item (by its archetype tag) as one of:

- **Met** - present and correct.
- **Partial** - present but weaker than the baseline (for example actions pinned
  by a mutable tag/major version instead of a SHA, or coverage collected but not
  gated).
- **Missing** - absent and applicable now.
- **N/A** - does not apply: the archetype tag excludes it, or a **temporal**
  item's milestone has not been reached (record why, for example "N/A until the
  first stable release").

Two scoring rules the cold cases need:

- **Temporal items** (marked temporal in the baseline, for example
  `EnablePackageValidation` "once a stable release exists") score **N/A with a
  note** before the milestone, not **Missing** - a pre-release library is not
  failing a post-release check.
- **Pick-one items** (a release record: `CHANGELOG.md` *or* curated GitHub
  Releases) score **Met** if either is present, **Partial** if neither is present
  yet but the repo must eventually choose - flag the pending choice in the report.

Produce a compact table, one row per domain, plus an itemized list of every
Partial and Missing finding. Keep it factual - cite the file and the line, not
an opinion.

## 3. Report gaps, highest-risk first

Order findings so the report leads with what matters. Use this risk order, which
follows the OpenSSF Scorecard risk levels in the
[citation catalog](references/best-practices.md):

1. **Critical / High** - no branch protection, a stored long-lived publishing
   key, no static analysis, missing license, a workflow that checks out and runs
   untrusted PR code with write scope, or actions referenced by a **floating
   ref** (a branch or `@latest`, which can change under you).
2. **Medium** - no dependency-update tool, no SECURITY.md, no package validation,
   missing SourceLink, no coverage gate, or actions pinned by a **mutable
   tag/major version** without a stated policy (weaker than a SHA, but not a
   floating ref - score Partial).
3. **Low** - missing community files (code of conduct, templates), no CHANGELOG,
   stylistic analyzer gaps.

Action pinning has a deliberate gray zone: a SHA pin is the recommended choice, a
floating branch ref is High, and a tag/major-version pin sits in between -
**Partial**, and an **accepted divergence** when the repo states it as policy
(see the catalog's divergence notes). Do not score a tag/major pin as Critical.

For each finding give: the gap, the baseline item it fails, the cited reason,
and the concrete remediation. Where the repository made a **deliberate** contrary
choice (GitHub Releases instead of a `CHANGELOG.md`; major-version action pins as
a stated policy), record it as an accepted divergence, not a defect - the
catalog's divergence notes describe the common ones. If the repo documents its
engineering choices (in `AGENTS.md`, a `docs/` philosophy page, or the README),
read that first - a mature repo's conscious omission is an accepted divergence,
not a gap.

## 4. Remediate in safe-to-risky order

Apply fixes in this order, so the reversible work lands first and the report
stays ahead of the risky steps:

1. **Local, additive, inert (apply directly).** Files that neither execute nor
   change build behavior on merge, and opt-in metadata: `.editorconfig`,
   `SECURITY.md`, `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md`, issue/PR templates,
   SourceLink and package-metadata properties, `ContinuousIntegrationBuild`,
   coverage collection. Validate after each (build, and the repo's agent-file
   gates for agent files).
2. **Local but behavior-changing (apply, then call out).** Anything that runs or
   changes behavior on merge, even though the file itself is reversible: a new
   workflow (a CodeQL or other CI job executes on the next push), a
   `dependabot.yml` (it opens PRs on its own), turning on `TreatWarningsAsErrors`,
   raising `AnalysisLevel`, pinning actions to SHAs, adding a coverage gate. These
   can break the build or CI or generate activity; do them in a reviewable commit
   and say so. Re-pin actions to SHAs using the version the repo already trusts,
   recording the version in a trailing comment.
3. **Remote or irreversible (propose, then run only on approval).** Branch
   protection / rulesets, secret-scanning and push-protection toggles, the
   trusted-publishing policy and the switch off a stored API key, the default
   branch, repository visibility. Emit the exact `gh` / API command or the ruleset
   JSON, show it, and wait for an explicit publishing verb. Never run these
   silently.

Batch related local fixes into coherent commits with clear messages. Do not
commit or push unless the user asks - preparing the changes is authorized;
publishing them is not.

## 5. Hand off specialized domains

- For domain 7 findings on code that handles untrusted input, run the security
  review skill rather than judging vulnerability shapes here.
- For domain 6 findings about runner choice, matrices, duplicate triggers,
  storage, or estimated spend, run the GitHub Actions cost optimization skill.
  This baseline establishes the required shape; that skill measures and tunes it.
- For domain 9 (vendoring the skill tier, wiring the validator and link checker,
  the drift loop), hand off to the skill-lifecycle skill and the fleet
  onboarding runbook - this skill confirms the gate exists; those own the how.

A consuming repository names both skills in its overlay.

## 6. Report back

Summarize: the per-domain score before and after, the fixes applied, the
behavior-changing commits to review, and the remote actions awaiting approval
with their exact commands. End with the single highest-value next step.
