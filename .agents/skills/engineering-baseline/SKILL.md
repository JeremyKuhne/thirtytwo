---
compatibility: Requires PowerShell 7.2, git, and the .NET SDK; remote setup additionally uses authenticated gh.
description: Create a new .NET repository fully wired for building, testing, and publishing with OSS best practices, or bring an existing repository up to a documented engineering baseline. Use when asked to "create a new repository" / "scaffold a new project" (a CLI tool, a class library / NuGet package, or a multi-target library) with CI, packaging, branch protection, and governance files, or to "ensure this repo follows modern engineering best practices" / "audit this repository" / "bring this repo up to standard". Scores a repository across nine domains - foundation, build, test, publish, versioning, CI, supply-chain security, OSS governance, and agent enablement - against an industry-cited baseline, then reports gaps and remediates. Irreversible or remote actions (repository creation, branch protection, release settings, publishing) are proposed for explicit approval, never run silently.
license: MIT
metadata:
    applicability: dotnet
    binding: optional-overlay
    github-path: skills/engineering-baseline
    github-pinned: v0.11.0
    github-ref: refs/tags/v0.11.0
    github-repo: https://github.com/JeremyKuhne/agent-skills
    github-tree-sha: b0e52e484192142c2b3083ee181b4f7a5b61b0d6
    maturity: canary
    portability: portable
    related: manage-skills, security-review, create-pr, github-actions-cost-optimization
    requires: none
    risk: remote-write
name: engineering-baseline
---
# Engineering baseline

If `overlay.md` exists beside this file, read it before acting; it contains
repository-specific bindings. This core remains usable without it.

The repository-lifecycle skill: stand up a new repository wired for the full
build / test / publish path with open-source best practices, or measure an
existing repository against the same standard and close the gaps. Both verbs
share one normative definition of "good" - the **baseline** - and one record of
the choices behind it, each traced to an authoritative industry source.

The standard in one paragraph: a high-quality .NET repository is **buildable**
(centralized, deterministic, analyzer-enforced), **tested** (with coverage and,
where it applies, perf and fuzz surfaces), **publishable** (signed, SourceLinked,
validated packages with rich metadata), **released** deterministically from tags,
**gated** by cost-aware CI with least-privilege tokens and pinned actions,
**secured** by branch protection, dependency updates, and scanning,
**governed** by the OSS community files, and **agent-enabled** with vendored
skills and the agent-file gates. The [baseline](baseline.md) is that standard as
a checklist; the [citation catalog](references/best-practices.md) is the *why*
behind every line.

## When to use

- "Create a new repository for a command-line tool", "scaffold a new library",
  "set up a new project with CI and publishing" - the greenfield path.
- "Ensure this repository follows modern engineering best practices", "audit
  this repo", "bring this repo up to standard", "what is missing for engineering
  fundamentals here" - the brownfield path.
- Before making a repository public, or onboarding it into a shared fleet, to
  confirm the governance and supply-chain gates are in place.

Not for vendoring or syncing the agent skills themselves - that is the
skill-lifecycle skill (a consuming repository names it in its overlay). This
skill *consumes* that one as the agent-enablement domain of the baseline.

## The two verbs

| Ask | Do | Detail |
| --- | --- | ------ |
| "audit this repo", "bring it up to standard" | Inventory the repo, score it by domain against the baseline, report gaps highest-risk-first, then remediate in safe-to-risky order. | [assess.md](assess.md) |
| "create a new repo", "scaffold a CLI tool / library" | Pick the archetype, lay down the standard structure, build/test/publish wiring, CI, governance files, and the agent gates, then propose the remote setup. | [scaffold.md](scaffold.md) |

Both measure against the same [baseline.md](baseline.md), and both end at the
same remote-setup boundary (below). Scaffold is "build to the baseline from
nothing"; assess is "diff an existing repo against the baseline and converge."

## The baseline domains

The baseline groups every check into nine domains, used identically by both
verbs and by the scoring report:

1. Repository foundation and licensing
2. Build configuration
3. Testing and coverage
4. Packaging and publishing
5. Versioning and releases
6. CI/CD
7. Supply-chain and security
8. OSS governance and community
9. Agent enablement

Each domain's concrete checks, the recommended choice, and the one-line rationale
live in [baseline.md](baseline.md). The external authority for each choice - and
the notes on where reasonable setups diverge - live in the
[citation catalog](references/best-practices.md).

## Guardrails: the remote boundary

Most of the baseline is local, reversible, and safe to apply directly: writing
`Directory.Build.props`, adding a workflow file, creating a `SECURITY.md`. Apply
those directly.

A few actions are remote or hard to reverse. **Never run these silently.**
Propose the exact command or setting, show it, and wait for an explicit
publishing verb before executing:

- Creating or renaming the remote repository (`gh repo create`).
- Branch protection or repository rulesets (`gh api`, ruleset JSON).
- Release, tag, and publishing settings, including trusted-publishing policies
  and the first package push.
- Anything that pushes commits, opens a PR, or cuts a release.

This mirrors the publish boundary in the repository's own agent guidance: the
work of preparing a change is authorized by the request; the publish is not.
When in doubt, stop and ask one yes/no question.

## Related skills

Run a security review over any code the scaffold generates or the assessment
adds, and use the repository's PR skills to publish the result. Hand detailed
workflow usage, billing, and matrix analysis to the GitHub Actions cost
optimization skill. The agent-enablement domain hands off to the skill-lifecycle
skill and the fleet onboarding runbook for vendoring the skill tier and wiring
the agent-file gates. A consuming repository wires these concrete
cross-references in its overlay.

## Sub-pages

- [baseline.md](baseline.md) - the nine-domain gold-standard checklist with the
  recommended choice and rationale for each item.
- [assess.md](assess.md) - the brownfield procedure: inventory, score, gap
  report, and safe-order remediation.
- [scaffold.md](scaffold.md) - the greenfield procedure: archetype selection and
  the build-to-baseline sequence for a CLI tool, library, or multi-target library.
- [references/best-practices.md](references/best-practices.md) - the citation
  catalog: every baseline choice traced to its industry source, with the
  rationale and the divergence notes.
