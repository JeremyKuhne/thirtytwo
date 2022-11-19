// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Windows.Support;
using Windows.Win32.Foundation;

namespace Tests.Windows.Support;

public class ErrorTests
{
    [Fact]
    public void Error_FormatMesage()
    {
        // Check an HRESULT with a product string that hopefully isn't localized.
        string message = Error.FormatMessage(HRESULT.FVE_E_LOCKED_VOLUME);
        message.Should().Contain("BitLocker");

        // .NET exception messages aren't localized. (Only .NET Framework)
        message = Error.FormatMessage(HRESULT.COR_E_OBJECTDISPOSED);
        message.Should().Be("Cannot access a disposed object.");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_ACCESS_DENIED);

        string asHResult = Error.FormatMessage(WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT());
        asHResult.Should().Be(message);

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE);
        message.Should().Contain("%1");

        string formatted = message.Replace("%1", "away");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE, args: "away");
        message.Should().Be(formatted);
    }
}