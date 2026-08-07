// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
public unsafe class WindowHandleValidationTests
{
    [TestMethod]
    public void Validate_ZeroHandle_Throws()
    {
        Action validate = () => WindowHandleValidation.Validate(0, Environment.ProcessId);

        validate.Should().Throw<InvalidDataException>();
    }

    [STATestMethod]
    public void Validate_ForeignProcessId_Throws()
    {
        using Window window = new(Window.DefaultBounds);
        int foreignProcessId = Environment.ProcessId == int.MaxValue ? int.MaxValue - 1 : Environment.ProcessId + 1;

        Action validate = () => WindowHandleValidation.Validate((long)window.Handle.Value, foreignProcessId);

        validate.Should().Throw<InvalidDataException>().WithMessage("*belongs to process*");
    }

    [STATestMethod]
    public void Validate_OwningProcess_ReturnsHandle()
    {
        using Window window = new(Window.DefaultBounds);

        HWND validated = WindowHandleValidation.Validate((long)window.Handle.Value, Environment.ProcessId);

        validated.Should().Be(window.Handle);
    }

    [STATestMethod]
    public void Validate_ForeignThreadId_Throws()
    {
        using Window window = new(Window.DefaultBounds);
        uint currentThreadId = PInvoke.GetCurrentThreadId();
        uint foreignThreadId = currentThreadId == uint.MaxValue ? currentThreadId - 1 : currentThreadId + 1;

        Action validate = () => WindowHandleValidation.Validate(
            (long)window.Handle.Value,
            Environment.ProcessId,
            foreignThreadId);

        validate.Should().Throw<InvalidDataException>().WithMessage("*belongs to thread*");
    }
}
