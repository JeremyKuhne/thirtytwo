// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;

namespace Windows.Messages;

[TestClass]
public class MouseHandlerTests
{
    [STATestMethod]
    public void ExtraButtonUp_X2_ForwardsButtonAndMouseState()
    {
        using Window window = new(Window.DefaultBounds);
        MouseHandler handler = new(window);
        MouseButton? observedButton = null;
        MouseKey? observedState = null;
        handler.MouseUp += (_, _, button, mouseState) =>
        {
            observedButton = button;
            observedState = mouseState;
        };

        window.SendMessage(
            MessageType.ExtraButtonUp,
            WPARAM.MAKEWPARAM((int)MouseKey.Shift, XButton2));

        observedButton.Should().Be(MouseButton.X2);
        observedState.Should().Be(MouseKey.Shift);
    }

    [STATestMethod]
    public void ExtraButtonDown_X2_ForwardsButtonAndMouseState()
    {
        using Window window = new(Window.DefaultBounds);
        MouseHandler handler = new(window);
        MouseButton? observedButton = null;
        MouseKey? observedState = null;
        handler.MouseDown += (_, _, button, mouseState) =>
        {
            observedButton = button;
            observedState = mouseState;
        };

        window.SendMessage(
            MessageType.ExtraButtonDown,
            WPARAM.MAKEWPARAM((int)MouseKey.Control, XButton2));

        observedButton.Should().Be(MouseButton.X2);
        observedState.Should().Be(MouseKey.Control);
    }

    [STATestMethod]
    public void OnButtonDown_X2_ForwardsButton()
    {
        using Window window = new(Window.DefaultBounds);
        MouseHandler handler = new(window);
        MouseButton? observedButton = null;
        handler.MouseDown += (_, _, button, _) => observedButton = button;

        handler.OnButtonDown(default, MouseButton.X2, default);

        observedButton.Should().Be(MouseButton.X2);
    }

    private const int XButton2 = 2;
}