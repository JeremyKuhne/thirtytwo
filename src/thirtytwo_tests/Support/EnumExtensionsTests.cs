// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

[TestClass]
public class EnumExtensionsTests
{
    [TestMethod]
    [DataRow(ByteFlags.One, ByteFlags.One, true)]
    [DataRow(default(ByteFlags), ByteFlags.One, false)]
    [DataRow(default(ByteFlags), default(ByteFlags), false)]
    [DataRow(ByteFlags.One, default(ByteFlags), false)]
    [DataRow(ByteFlags.One, (ByteFlags)0xFF, true)]
    [DataRow((ByteFlags)0xFF, (ByteFlags)0xFF, false)]
    [DataRow((ByteFlags)0xFF, (ByteFlags)0x00, false)]
    [DataRow((ByteFlags)0x00, (ByteFlags)0xFF, false)]
    [DataRow((ByteFlags)0xFF, (ByteFlags)0b1000_0000, true)]
    [DataRow((ByteFlags)0xFF, (ByteFlags)0b0000_0001, true)]
    [DataRow((ByteFlags)0xFF, (ByteFlags)0b1000_0001, false)]
    public void EnumExtensions_IsOnlyOneFlagSet_ByteFlags(ByteFlags value, ByteFlags flag, bool expected)
    {
        value.IsOnlyOneFlagSet(flag).Should().Be(expected);
    }

    [Flags]
    public enum ByteFlags : byte
    {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8
    }
}