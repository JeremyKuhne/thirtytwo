// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Win32.Foundation;

namespace Windows101;

internal partial class Program
{
    private class HelloWindowClass : WindowClass
    {
        // Overriding the callback method will allow us to provide our own custom behavior
        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                // The Paint message is sent when the Window contents need drawn.
                case MessageType.Paint:

                    PaintMessage(window, "Hello .NET and Win32!");

                    // Return 0 to indicate we've handled the message
                    return (LRESULT)0;
            }

            // Let the base class handle any other messages
            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}