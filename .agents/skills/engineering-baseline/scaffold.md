# Scaffold a new repository

The greenfield verb: stand up a new repository built to [baseline.md](baseline.md)
from nothing. Triggered by "create a new repository for a command-line tool",
"scaffold a new library", or "set up a new project with CI and publishing".

The primary mechanism is the scaffold script ([scripts/New-DotnetRepo.ps1](scripts/New-DotnetRepo.ps1)),
which uses `dotnet new` for the project skeleton and then applies the hardening
layer (centralized build, CI workflows, governance files, agent stubs). Running
the script produces an immediately buildable, testable, and packable tree. Stop
at the remote boundary and propose those steps for explicit approval.

## 1. Confirm the archetype and identity

Pick the archetype - it selects which baseline items and script parameters apply:

| Archetype | Ships | Key script parameters |
| --------- | ----- | --------------------- |
| **tool** | A `dotnet` tool executable | `-Archetype tool -ToolCommandName <cmd>` |
| **library** | A single-TFM NuGet library | `-Archetype library` |
| **multi-target** | A library across a modern + legacy TFM | `-Archetype multi-target -Framework net10.0 -FrameworkLegacy net481` |

Confirm, before running the script, with one short prompt: the repository name,
the target framework(s), the package id and one-line description, the license
(default MIT), and the owner. Do not invent these - if the user did not supply
them, ask once. These are the literals the script takes as parameters, and what
a brownfield repo would keep in its overlay.

## 2. Run the scaffold script

Invoke `New-DotnetRepo.ps1` from the skill's `scripts/` directory (or a
vendored copy in the consuming repo). Use an empty directory outside any
existing repository as the root - the script refuses to run in a non-empty
directory.

The static file bodies it lays down are real files under `scripts/template/`
(rendered with simple `{{TOKEN}}` substitution); the `.ps1` is the orchestration
layer that runs `dotnet new`, renders the template, and applies project-file
hardening. To change what a scaffolded repo contains, edit the template files,
not here-strings.

```pwsh
# Example: scaffold a dotnet tool
pwsh /path/to/New-DotnetRepo.ps1 `
    -Root   C:\src\widgettool `
    -Name   widgettool `
    -Archetype tool `
    -PackageId     WidgetTool `
    -ToolCommandName widgettool `
    -Description   "A sample command-line tool." `
    -Owner  YourGitHubHandle
```

The script produces: `global.json`, `Directory.Build.props/.targets`,
`Directory.Packages.props` (CPM on), a `dotnet new`-generated project and test
project, `.gitignore`/`.gitattributes`/`.editorconfig`, `README.md`, `LICENSE`,
`.github/workflows/` (automatic CI, manual Full CI, publish, CodeQL, and the
agent mirror), `dependabot.yml`, `SECURITY.md`, `CODE_OF_CONDUCT.md`,
`CONTRIBUTING.md`, issue/PR templates, and `AGENTS.md` with its generated Copilot
mirror.

The workflow defaults are cost-aware without weakening the merge gate. A pull
request or merge group gets one Release build/test job: standard Linux for
modern-only projects and Windows when a legacy target framework must compile
there. Maintainers run the manually dispatched Full CI workflow before every
release and after SDK or build-tool changes; it covers Debug and Release in one
job. CodeQL runs on pull requests, merge groups, and a weekly schedule. For a
private repository with GitHub Code Security, enable it before the first pull
request and require CodeQL results in the ruleset. Without that entitlement,
replace CodeQL with a supported scanner and require its status check instead.
None of these workflows repeats automatically after a strictly protected merge.
If the remote ruleset later permits direct or bypass pushes, restore
default-branch push triggers in CI, CodeQL (or its replacement), and the agent
mirror before relying on that path.

It also standardizes output paths to a top-level `artifacts/` tree:

- `artifacts/bin/` - build outputs
- `artifacts/obj/` - intermediates
- `artifacts/packages/` - packed `.nupkg` output

The scaffold is **born safe** on the supply-chain axis: a committed
`packages.lock.json` (restored with `--locked-mode` in CI), NuGet audit over
direct and transitive packages, and a `nuget.config` that maps every package to
nuget.org to block dependency-confusion. Package versions are **pinned from the
vetted manifest `scripts/versions.json`** - a known-good floor, never "latest".
Refresh that floor deliberately with `scripts/Update-ScaffoldVersions.ps1`, which
proposes only versions that pass a quarantine window plus advisory, deprecation,
listing, and license-allowlist checks (and can scaffold-and-build to confirm TFM
compatibility); review the diff before committing. The GitHub Actions pinned in
the workflow templates carry the same quarantine floor, refreshed by
`scripts/Update-ScaffoldActions.ps1` (the actions counterpart). Keeping both
floors current, plus a `dependabot.yml` that **groups every update per ecosystem
into one PR**, is what stops a fresh repo from opening a long list of day-one
dependency bumps.

Test projects run on the **Microsoft Testing Platform** (MTP) and default to
**MSTest** (`-TestRunner xunit` switches to xunit.v3); both run at the fullest
parallelism by default, and `global.json` selects the MTP runner.

The shippable project is marked **AOT/trim clean** (`IsAotCompatible`) on its
modern target, so the trim, AOT, and single-file analyzers run by default and the
code stays composable into AOT apps even when AOT is never published.

## 3. Pin the action SHAs (domain 6)

The workflows emit `<SHA>` placeholders for every third-party action, each with
its target version in a trailing comment kept current by
`scripts/Update-ScaffoldActions.ps1`. Replace each placeholder with the full
commit SHA for the commented version before the first push, then let Dependabot's
grouped `github-actions` updates keep them current. Use
[StepSecurity](https://app.stepsecurity.io) or
`gh api repos/<owner>/<repo>/git/refs/tags` to resolve each version to a SHA.

This is a local, reversible edit - do it before `git init`.

## 4. Extend for multi-target (domain 2, conditional)

For `multi-target`, the script adds the framework pair to `Directory.Build.props`
and switches the main project to `<TargetFrameworks>`. The polyfill package
strategy for the legacy target (PolySharp, `Microsoft.Bcl.*`, `System.Memory`,
etc.) is not added by the script - hand that off to the polyfill skill a
consuming repository names in its overlay.

## 5. Add optional surfaces (domain 3, conditional)

The script does not scaffold a perf or fuzz project; add those only if the
tool/library has a hot path or an untrusted-input surface. Hand them off to the
perf-testing and fuzz-testing skills a consuming repository names in its overlay.

## 6. Wire agent enablement (domain 9)

The script creates an `AGENTS.md` stub, generates its Copilot mirror
(`.github/copilot-instructions.md`) with `tools/Sync-AgentInstructions.ps1`, adds
an `agent-files.yml` workflow that fails if the mirror drifts from `AGENTS.md`,
and vendors a pinned starting skill tier into `.agents/skills/` via `gh skill`
(`manage-skills`, `agent-files-review`, `create-pr`, `address-pr-feedback`,
and `security-review` by default; override with `-Skills` / `-SkillsRef`). When
`gh skill` is unavailable it records the pinned install commands instead of
failing. Vendor the GitHub Actions cost optimization skill when the repository
needs a measured workflow audit beyond these defaults. The broader agent-file
gate (skill-frontmatter validation, link checking) and vendoring any domain
skills remain a handoff to the skill-lifecycle skill and the fleet onboarding
runbook - do not reinvent that pipeline here.

## 7. Validate locally

```pwsh
dotnet build -c Release
dotnet test  -c Release
dotnet pack  src/<Name>/<Name>.csproj -c Release -o artifacts/packages
```

All three must be clean before going near the remote. The pack confirms the
package metadata is correct and SourceLink is wired. Fix anything red first.

## 8. Propose the remote setup (the boundary)

The script prints a remote setup checklist at the end. Everything on it is
remote or hard to reverse - **propose each step and wait for an explicit
publishing verb.** Do not run them silently:

- Replace `<SHA>` placeholders in workflows (see step 3).
- `gh repo create <owner>/<name> --public --source . --remote origin`
- `git init && git add -A && git commit -m "Initial scaffold" && git push -u origin main`
- For a private repository, enable GitHub Code Security before the first pull
    request. If it is unavailable, replace CodeQL with a supported scanner.
- Branch protection / ruleset on `main`: require `build`, strict up-to-date
    branches or a merge queue, and either CodeQL code-scanning results at
    documented thresholds or the replacement scanner's status check; require pull
    requests with no bypass path; block force-push and deletion. If a bypass path
    is required, restore default-branch push validation in CI, CodeQL or its
    replacement, and the agent mirror first. Emit the exact `gh api` call or
    ruleset JSON for review.
- Enable secret scanning and push protection (GitHub repo Settings > Security).
- Register the trusted-publishing policy on nuget.org before the first publish.

Present these as a numbered checklist, then stop.

## 9. Report back

Summarize the archetype, the tree created, what was validated locally, and the
remote checklist still awaiting approval. End with the single next action.
