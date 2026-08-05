# thirtytwo

[![Build](https://github.com/JeremyKuhne/thirtytwo/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JeremyKuhne/thirtytwo/actions/workflows/dotnet.yml)

An experimental .NET object model for Win32, built on
[CsWin32](https://github.com/microsoft/CsWin32). It builds on the ideas in
[WInterop](https://github.com/JeremyKuhne/WInterop) while focusing on
higher-level abstractions for windows, controls, graphics, accessibility, and
COM.

## Status

Thirtytwo is under active development and has not published a supported NuGet
package or stable release. APIs may change without notice.

## Requirements

- Windows
- An x64 .NET SDK capable of targeting `net10.0-windows`

## Build and test

```pwsh
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --report-trx
```

The solution includes Win32 sample applications under `src/samples/`. Some
tests interact with desktop resources such as the clipboard and therefore
require an interactive Windows session.

## Contributing and security

See [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a change. Report
security issues privately as described in [SECURITY.md](SECURITY.md).

## License

Thirtytwo is licensed under the [MIT License](LICENSE). Third-party attribution
notices are listed in [THIRD-PARTY-NOTICES.TXT](THIRD-PARTY-NOTICES.TXT).
