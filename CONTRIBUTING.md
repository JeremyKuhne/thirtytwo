# Contributing to thirtytwo

Thanks for your interest in contributing.

## Prerequisites

- Windows with an interactive desktop session
- An x64 .NET SDK capable of targeting `net10.0-windows`
- Git

## Build and test

```pwsh
git clone https://github.com/JeremyKuhne/thirtytwo.git
cd thirtytwo
dotnet restore
dotnet build --configuration Debug --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --report-trx
```

The file-dialog manual test is skipped during automated runs. Clipboard and
other desktop-resource tests require an interactive Windows session.

## Submitting changes

1. Fork the repository and create a focused branch from `main`.
2. Keep changes consistent with the existing object model and CsWin32
   projections.
3. Add or update tests for every behavior change and public API.
4. Run the Release build and test commands above.
5. Open a pull request against `main` and explain the behavior and validation.

Changes under `.agents/` must also pass the validation commands documented in
[the skills catalog](.agents/skills/README.md#validation).

## License

By submitting a pull request, you agree that your contribution is licensed
under the [MIT License](LICENSE) that governs this project.
