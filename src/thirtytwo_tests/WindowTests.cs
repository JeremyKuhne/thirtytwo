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

    [STATestMethod]
    public unsafe void OnDpiChanged_Message_UpdatesBoundsCachesAndCallback()
    {
        using DpiTrackingWindow window = new(new Rectangle(40, 50, 320, 240));
        uint oldDpi = window.GetDpi();
        ushort newDpi = checked((ushort)(oldDpi + 24));
        Rectangle suggestedBounds = new(40, 50, 400, 300);
        RECT suggestedRectangle = suggestedBounds;
        nuint packedDpi = newDpi | ((nuint)newDpi << 16);

        // SendMessage is synchronous, so the stack RECT remains valid until the window procedure returns.
        LRESULT result = window.SendMessage(
            MessageType.DpiChanged,
            (WPARAM)packedDpi,
            (LPARAM)(nint)(&suggestedRectangle));

        result.Value.Should().Be(0);
        window.GetWindowRectangle().Should().Be(suggestedBounds);
        window.OldDpi.Should().Be(oldDpi);
        window.NewDpi.Should().Be(newDpi);
        window.DpiChangeCount.Should().Be(1);
        window.PixelToHiMetric((int)newDpi).Should().Be(2540);
    }

    [STATestMethod]
    public void OnDpiChanged_AfterParent_UsesCapturedDpiAndInvokesCallback()
    {
        using DpiTrackingWindow window = new(new Rectangle(40, 50, 320, 240));
        uint newDpi = window.GetDpi();
        uint oldDpi = newDpi == 96 ? 120u : newDpi - 24;
        dynamic accessor = ((Window)window).TestAccessor.Dynamic;
        accessor._lastDpi = oldDpi;

        _ = window.SendMessage(MessageType.DpiChangedBeforeParent);
        accessor._lastDpi = newDpi;
        _ = window.SendMessage(MessageType.DpiChangedAfterParent);

        window.OldDpi.Should().Be(oldDpi);
        window.NewDpi.Should().Be(newDpi);
        window.DpiChangeCount.Should().Be(1);
        window.PixelToHiMetric((int)newDpi).Should().Be(2540);
    }

    [TestMethod]
    public void DpiChanged_NullSuggestedBounds_Throws()
    {
        Action action = static () => _ = new Message.DpiChanged(default, default);

        action.Should().Throw<ArgumentNullException>();
    }

    [STATestMethod]
    public void DpiChanged_NullSuggestedBounds_DoesNotEscapeWindowProcedure()
    {
        using DpiTrackingWindow window = new(new Rectangle(40, 50, 320, 240));

        LRESULT result = window.SendMessage(MessageType.DpiChanged);

        result.Value.Should().Be(0);
        window.DpiChangeCount.Should().Be(0);
    }

    [STATestMethod]
    public void DpiChanged_RegisteredControl_ForwardsWithoutEscapingWindowProcedure()
    {
        using Window window = new(Window.DefaultBounds);
        using ButtonControl button = new(parentWindow: window);

        Action action = () =>
        {
            _ = button.SendMessage(MessageType.DpiChangedBeforeParent);
            _ = button.SendMessage(MessageType.DpiChanged);
            _ = button.SendMessage(MessageType.DpiChangedAfterParent);
        };

        action.Should().NotThrow();
    }

    [STATestMethod]
    public void IsSubclassed_FrameworkOwnedWindow_ReturnsFalse()
    {
        using WindowClassTrackingWindow window = new();

        window.IsWindowClassSubclassed.Should().BeFalse();
    }

    [STATestMethod]
    public void IsSubclassed_RegisteredControl_ReturnsTrue()
    {
        using Window window = new(Window.DefaultBounds);
        using WindowClassTrackingButton button = new(window);

        button.IsWindowClassSubclassed.Should().BeTrue();
    }

    private sealed class DpiTrackingWindow(Rectangle bounds) : Window(bounds)
    {
        internal uint OldDpi { get; private set; }

        internal uint NewDpi { get; private set; }

        internal int DpiChangeCount { get; private set; }

        protected override void OnDpiChanged(uint oldDpi, uint newDpi)
        {
            OldDpi = oldDpi;
            NewDpi = newDpi;
            DpiChangeCount++;
            base.OnDpiChanged(oldDpi, newDpi);
        }
    }

    private sealed class WindowClassTrackingWindow : Window
    {
        internal bool IsWindowClassSubclassed => _windowClass.IsSubclassed;
    }

    private sealed class WindowClassTrackingButton(Window parentWindow) : ButtonControl(parentWindow: parentWindow)
    {
        internal bool IsWindowClassSubclassed => _windowClass.IsSubclassed;
    }

}