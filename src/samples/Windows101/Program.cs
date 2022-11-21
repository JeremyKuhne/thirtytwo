// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Windows101;

internal class Program
{
    // To create a Windows Application in .NET Core you must do the following things:
    //
    //  1. Create a .NET Core Console App.
    //  2. Double click on the project and change <OutputType> to WinExe.
    //  3. Mark the Main method as [STAThread].

    // Optional- to make things look better, add an Application Manifest file and
    // change it to match the one included in this project.

    [STAThread]
    private static void Main()
    {
        //  You can just show dialog boxes to interact
        Application.ShowTaskDialog("Hello World", "Hello from .NET and Win32!", title: "Hello");

        //  Or create actual Window classes and run them. A Window Class in Windows
        //  includes a few basic things:
        //
        //   1. Appearance settings (border, icon, background, etc.)
        //   2. A callback pointer for messages (mouse, keyboard, etc.)
        //   3. An optional menu
        //
        //  The Window Class is a template that actual Window instances are created
        //  from. WInterop wraps the registration and callbacks in "WindowClass"
        //  that you can derive from. "CreateMainWindowAndRun" will create an
        //  instance of the Window for the given WindowClass and loop processing
        //  messages until the Window is closed.

        Application.Run(new WindowClass(), windowTitle: "So Simple");

        //  To display a message in a Window you have to draw it yourself in response
        //  to a message to draw the window contents.
        Application.Run(new HelloWindowClass(), windowTitle: "Hello!");
    }

    private class HelloWindowClass : WindowClass
    {
        // Overriding the callback method will allow us to provide our own custom behavior
        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                // The Paint message is sent when the Window contents need drawn.
                case MessageType.Paint:

                    // Drawing is done in a Device Context by calling BeginPaint(). When the
                    // DeviceContext is disposed it will call EndPaint().
                    using (DeviceContext dc = window.BeginPaint())
                    {
                        Rectangle client = window.GetClientRectangle();

                        // In a using statement to delete the object after we create it.
                        // This is a bit wasteful, but for simplicity sake we're not caching.
                        using HFONT font = HFONT.CreateFont(
                            height: client.Height / 5,
                            family: FontFamilyType.Swiss);

                        // In a using to put back the original font.
                        using var selectionScope = dc.SelectObject(font);

                        // Draw the given text in the middle of the client area of the Window.
                        dc.DrawText(
                            "Hello .NET and Win32!",
                            bounds: client,
                            DrawTextFormat.SingleLine | DrawTextFormat.Center | DrawTextFormat.VerticallyCenter);
                    }

                    // Return 0 to indicate we've handled the message
                    return (LRESULT)0;
            }

            // Let the base class handle any other messages
            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
