// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.Controls;

[TestClass]
public class ButtonControlTests
{
    [TestMethod]
    public void CheckState_RoundTripsSupportedValues()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ButtonControl button = new(
            buttonStyle: ButtonControl.Styles.AutoThreeState,
            parentWindow: window);

        foreach (ButtonCheckState state in Enum.GetValues<ButtonCheckState>())
        {
            button.CheckState = state;
            button.CheckState.Should().Be(state);
        }
    }

    [TestMethod]
    public void CheckState_InvalidValue_Throws()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ButtonControl button = new(parentWindow: window);

        Action action = () => button.CheckState = (ButtonCheckState)uint.MaxValue;

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Click_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ButtonControl button = new(parentWindow: window);
        int clickCount = 0;
        button.Click += (sender, eventArgs) => clickCount++;

        window.SendMessage(
            MessageType.Command,
            WPARAM.MAKEWPARAM(0, (int)PInvoke.BN_CLICKED),
            (LPARAM)button.Handle);

        clickCount.Should().Be(1);
    }
}