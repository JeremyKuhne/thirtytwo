// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Foundation;

public class HRESULTTests
{
    [Fact]
    public void HRESULT_ToStringWithDescription()
    {
        // .NET exception messages aren't localized. (Only .NET Framework)
        string message = HRESULT.COR_E_OBJECTDISPOSED.ToStringWithDescription();
        message.Should().Be("HRESULT 0x80131622 [-2146232798]: Cannot access a disposed object.");

        message = WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT().ToStringWithDescription();
        message.Should().StartWith("HRESULT 0x80070005 [ERROR_ACCESS_DENIED (5)]:");
    }
}
