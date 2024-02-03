// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public class BstrBufferTests
{
    [Fact]
    public unsafe void BstrBuffer_Basics()
    {
        BSTR bstr = new("Foo");
        using BstrBuffer buffer = new(2);
        buffer[0] = bstr;
        ((nint)buffer[0].Value).Should().Be((nint)bstr.Value);
        buffer[0].Dispose();
        buffer[0].IsNull.Should().BeTrue();
    }

    [Fact]
    public void BstrBuffer_Clear_FreesBstrs()
    {
        using BstrBuffer buffer = new(2);
        buffer[0] = new("Foo");
        buffer[1] = new("Bar");
        buffer.Clear();
        buffer[0].IsNull.Should().BeTrue();
        buffer[1].IsNull.Should().BeTrue();
    }
}
