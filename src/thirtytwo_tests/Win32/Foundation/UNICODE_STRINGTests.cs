// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

[TestClass]
public unsafe class UNICODE_STRINGTests
{
    [TestMethod]
    public void MaximumLengthInChars_UsesMaximumLength()
    {
        char* buffer = stackalloc char[4];
        UNICODE_STRING value = new()
        {
            Buffer = (PWSTR)buffer,
            Length = 2 * sizeof(char),
            MaximumLength = 4 * sizeof(char)
        };

        value.LengthInChars.Should().Be(2);
        value.MaximumLengthInChars.Should().Be(4);
        value.CurrentValue.Length.Should().Be(2);
        value.FullBuffer.Length.Should().Be(4);
    }
}