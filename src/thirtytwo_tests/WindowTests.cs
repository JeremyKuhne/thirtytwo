// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows;

[TestClass]
public class WindowTests
{
    [STATestMethod]
    public unsafe void Dispose_CreatedWindow_ClearsHandleAndLookup()
    {
        using Window window = new(new Rectangle(0, 0, 100, 100));
        HWND handle = window.Handle;
        Window.FromHandle(handle).Should().BeSameAs(window);

        window.Dispose();

        window.Handle.Should().Be(HWND.Null);
        Window.FromHandle(handle).Should().BeNull();
        PInvoke.GetWindowThreadProcessId(handle, null).Should().Be(0);
        window.Dispose();
    }
}