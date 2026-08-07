// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI.IntegrationHarness;

internal static unsafe class WindowHandleValidation
{
    internal static HWND Validate(long windowHandle, int expectedProcessId, uint expectedThreadId = 0)
    {
        if (windowHandle <= 0)
        {
            throw new InvalidDataException($"Window handle '{windowHandle}' is invalid.");
        }

        if (expectedProcessId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedProcessId));
        }

        HWND window = new(checked((nint)windowHandle));
        uint ownerProcessId;
        uint ownerThreadId = PInvoke.GetWindowThreadProcessId(window, &ownerProcessId);
        if (ownerThreadId == 0)
        {
            throw new InvalidDataException($"Window handle '0x{windowHandle:x}' is not valid.");
        }

        if (ownerProcessId != (uint)expectedProcessId)
        {
            throw new InvalidDataException(
                $"Window handle '0x{windowHandle:x}' belongs to process {ownerProcessId}, not {expectedProcessId}.");
        }

        if (expectedThreadId != 0 && ownerThreadId != expectedThreadId)
        {
            throw new InvalidDataException(
                $"Window handle '0x{windowHandle:x}' belongs to thread {ownerThreadId}, not {expectedThreadId}.");
        }

        return window;
    }
}
