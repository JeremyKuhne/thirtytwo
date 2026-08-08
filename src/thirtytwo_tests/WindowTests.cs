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

    [STATestMethod]
    public unsafe void ControlColor_DefaultBackground_InheritsParentColor()
    {
        Color backgroundColor = Color.FromArgb(241, 245, 249);
        using MainWindow window = new(Window.DefaultBounds, backgroundColor: backgroundColor);
        using CustomControl container = new(parentWindow: window);
        using StaticControl label = new(text: "Label", parentWindow: container);
        using ButtonControl checkBox = new(
            text: "Check box",
            buttonStyle: ButtonControl.Styles.AutoCheckBox,
            parentWindow: container);
        using DeviceContext labelContext = label.GetDeviceContext();
        using DeviceContext checkBoxContext = checkBox.GetDeviceContext();

        LRESULT labelBrush = container.SendMessage(
            MessageType.ControlColorStatic,
            (WPARAM)(nuint)labelContext.Handle.Value,
            (LPARAM)label.Handle);
        LRESULT checkBoxBrush = container.SendMessage(
            MessageType.ControlColorButton,
            (WPARAM)(nuint)checkBoxContext.Handle.Value,
            (LPARAM)checkBox.Handle);

        labelContext.GetBackgroundColor().ToArgb().Should().Be(backgroundColor.ToArgb());
        checkBoxContext.GetBackgroundColor().ToArgb().Should().Be(backgroundColor.ToArgb());
        labelBrush.Value.Should().NotBe(0);
        checkBoxBrush.Value.Should().Be(labelBrush.Value);
    }

}