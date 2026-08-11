# Minimal WinUI 3 island host

This code-only .NET sample owns a raw Win32 HWND and message loop, then hosts
WinUI 3 content with `DesktopWindowXamlSource`. It is pinned to Windows App SDK
2.3.1 and targets Windows 10 version 1809 or later.

Build both documented architectures:

```pwsh
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-x64
dotnet build MinimalWinUIHost.csproj --configuration Release --runtime win-arm64
```

Run the x64 sample interactively:

```pwsh
dotnet run --project MinimalWinUIHost.csproj --configuration Release --runtime win-x64
```

Inspect keyboard, pointer, resize, focus, and shutdown behavior in the running
window. The machine must have the matching Windows App Runtime and Visual C++
Redistributable installed.
