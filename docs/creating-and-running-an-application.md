# Creating and running an Application

This guide covers the structure and lifetime of a thirtytwo application: create
a Windows executable, construct a root window, run the UI message loop, handle
window messages, and choose who disposes the window.

## Create the executable project

Use a Windows executable that targets the framework supported by thirtytwo and
references the thirtytwo project or package. A minimal project file contains:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="path\to\thirtytwo.csproj" />
  </ItemGroup>
</Project>
```

Replace the project-reference path with the path from the application project
to `src/thirtytwo/thirtytwo.csproj`.

## Create and run a root window

Mark the entry point with `[STAThread]` and give `Application.Run` a factory for
the top-level `MainWindow`:

```csharp
using Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.UseVisualStyles = true;
        Application.Run(
            () => new MainWindow(title: "My thirtytwo application"));
    }
}
```

`Application.Run` starts the dispatcher before invoking the factory. The root
window and any controls it creates can therefore use `Window.Dispatcher` during
construction and synchronous creation messages. `Application.Run` then shows
and updates the root window and processes messages until that window is
destroyed. Finally, it shuts down the dispatcher and disposes the window.

`UseVisualStyles` defaults to `true`; set it before `Application.Run` when an
application needs a different policy.

`[STAThread]` is required by common Windows UI facilities such as COM dialogs
and clipboard operations.

## Customize the root window

Derive from `MainWindow` to create controls, retain application state, or handle
native messages. Override `WindowProcedure` for messages owned by the window and
delegate unhandled messages to the base implementation:

```csharp
using Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

internal sealed class ApplicationWindow : MainWindow
{
    public ApplicationWindow()
        : base(title: "Paint example")
    {
    }

    protected override LRESULT WindowProcedure(
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        if (message == MessageType.Paint)
        {
            using DeviceContext deviceContext = window.BeginPaint();
            deviceContext.DrawText(
                "Hello from thirtytwo",
                window.GetClientRectangle(),
                DrawTextFormat.SingleLine
                    | DrawTextFormat.Center
                    | DrawTextFormat.VerticallyCenter);

            return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
```

Run the derived window in the same way:

```csharp
Application.Run(() => new ApplicationWindow());
```

Dispose child controls, brushes, fonts, and other resources owned by the window
from its `Dispose(bool)` override before calling the base implementation.

## Use a WindowClass directly

For lower-level scenarios, `Application.Run` can create the root `Window` from
a `WindowClass`:

```csharp
using Windows;

WindowClass windowClass = new();
Application.Run(windowClass, windowTitle: "WindowClass application");
```

Derive from `WindowClass` and override its `WindowProcedure` when the behavior
belongs to the registered native window class rather than one managed `Window`
instance. The overload that accepts a `Rectangle` also sets the initial bounds.

## Control window ownership

By default, `Application.Run` disposes the supplied window. Pass
`disposeWindow: false` when the caller owns its managed lifetime:

```csharp
using MainWindow window = new(title: "Caller-owned window");
Application.Run(window, disposeWindow: false);
```

Closing the root window still destroys its native handle and ends the message
loop. The option changes who disposes the managed object; it does not keep the
native window alive after it is closed.

## Run applications sequentially

Only one outer message loop can run on a thread at a time. After one call
returns, the same thread can run another window with a fresh dispatcher:

```csharp
Application.Run(() => new MainWindow(title: "First window"));
Application.Run(() => new MainWindow(title: "Second window"));
```

Do not call `Application.Run` recursively from a window callback. Use dialogs
for modal UI and use the [dispatcher](dispatching.md) to schedule later work on
the active UI thread.