// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Text;

namespace Windows.Win32.System.Registry;

[TestClass]
public class RegistryValueEntryTests
{
    [TestMethod]
    public void Strings_UnreadOrNonString_IsEmpty()
    {
        ReadStrings(REG_VALUE_TYPE.REG_SZ, Utf16("value"), hasData: false).Should().BeEmpty();
        ReadStrings(REG_VALUE_TYPE.REG_BINARY, Utf16("value"), hasData: true).Should().BeEmpty();
    }

    [TestMethod]
    public void Strings_SingleStringTypes_ReturnOneString()
    {
        foreach (REG_VALUE_TYPE type in new[]
        {
            REG_VALUE_TYPE.REG_SZ,
            REG_VALUE_TYPE.REG_EXPAND_SZ,
            REG_VALUE_TYPE.REG_LINK,
        })
        {
            ReadStrings(type, Utf16("value\0")).Should().Equal("value");
        }
    }

    [TestMethod]
    public void Strings_EmptySingleString_ReturnsOneEmptyString()
    {
        ReadStrings(REG_VALUE_TYPE.REG_SZ, []).Should().Equal(string.Empty);
        ReadStrings(REG_VALUE_TYPE.REG_SZ, Utf16("\0")).Should().Equal(string.Empty);
    }

    [TestMethod]
    public void Strings_SingleString_PreservesEmbeddedNullAndTrimsAtMostOneTerminator()
    {
        ReadStrings(REG_VALUE_TYPE.REG_SZ, Utf16("first\0second\0\0"))
            .Should().Equal("first\0second\0");
    }

    [TestMethod]
    public void Strings_SingleString_ToleratesMissingTerminatorAndOddTrailingByte()
    {
        byte[] bytes = [.. Utf16("value"), 0xFF];

        ReadStrings(REG_VALUE_TYPE.REG_SZ, bytes).Should().Equal("value");
    }

    [TestMethod]
    public void Strings_MultiString_ReturnsSegmentsUntilEmptySegment()
    {
        ReadStrings(REG_VALUE_TYPE.REG_MULTI_SZ, Utf16("first\0second\0\0ignored"))
            .Should().Equal("first", "second");
    }

    [TestMethod]
    public void Strings_MultiString_ToleratesMissingTerminatorAndOddTrailingByte()
    {
        byte[] bytes = [.. Utf16("first\0second"), 0xFF];

        ReadStrings(REG_VALUE_TYPE.REG_MULTI_SZ, bytes).Should().Equal("first", "second");
    }

    [TestMethod]
    public void Strings_EmptyMultiString_IsEmpty()
    {
        ReadStrings(REG_VALUE_TYPE.REG_MULTI_SZ, []).Should().BeEmpty();
        ReadStrings(REG_VALUE_TYPE.REG_MULTI_SZ, Utf16("\0")).Should().BeEmpty();
        ReadStrings(REG_VALUE_TYPE.REG_MULTI_SZ, Utf16("\0\0")).Should().BeEmpty();
    }

    [TestMethod]
    public void TryReadUInt32_DecodesBothByteOrders()
    {
        byte[] littleEndian = new byte[sizeof(uint)];
        byte[] bigEndian = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(littleEndian, 0x12345678);
        BinaryPrimitives.WriteUInt32BigEndian(bigEndian, 0x12345678);

        CreateEntry(REG_VALUE_TYPE.REG_DWORD, littleEndian).TryReadUInt32(out uint littleValue).Should().BeTrue();
        CreateEntry(REG_VALUE_TYPE.REG_DWORD_BIG_ENDIAN, bigEndian).TryReadUInt32(out uint bigValue).Should().BeTrue();
        littleValue.Should().Be(0x12345678);
        bigValue.Should().Be(0x12345678);
    }

    [TestMethod]
    public void TryReadUInt64_WidensBothDwordByteOrders()
    {
        byte[] littleEndian = new byte[sizeof(uint)];
        byte[] bigEndian = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(littleEndian, uint.MaxValue);
        BinaryPrimitives.WriteUInt32BigEndian(bigEndian, uint.MaxValue);

        CreateEntry(REG_VALUE_TYPE.REG_DWORD, littleEndian).TryReadUInt64(out ulong littleValue).Should().BeTrue();
        CreateEntry(REG_VALUE_TYPE.REG_DWORD_BIG_ENDIAN, bigEndian).TryReadUInt64(out ulong bigValue).Should().BeTrue();
        littleValue.Should().Be(uint.MaxValue);
        bigValue.Should().Be(uint.MaxValue);
    }

    [TestMethod]
    public void TryReadUInt64_DecodesQword()
    {
        byte[] data = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(data, 0x0123456789ABCDEF);

        CreateEntry(REG_VALUE_TYPE.REG_QWORD, data).TryReadUInt64(out ulong value).Should().BeTrue();
        value.Should().Be(0x0123456789ABCDEF);
    }

    [TestMethod]
    public void TryReadUInt32_DoesNotNarrowQword()
    {
        byte[] data = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(data, uint.MaxValue);

        CreateEntry(REG_VALUE_TYPE.REG_QWORD, data).TryReadUInt32(out uint value).Should().BeFalse();
        value.Should().Be(0);
    }

    [TestMethod]
    public void NumericReaders_RejectUnreadMismatchedAndInexactData()
    {
        CreateEntry(REG_VALUE_TYPE.REG_DWORD, new byte[sizeof(uint)], hasData: false)
            .TryReadUInt32(out uint unread).Should().BeFalse();
        CreateEntry(REG_VALUE_TYPE.REG_BINARY, new byte[sizeof(uint)])
            .TryReadUInt32(out uint wrongType).Should().BeFalse();
        CreateEntry(REG_VALUE_TYPE.REG_DWORD, new byte[sizeof(uint) - 1])
            .TryReadUInt32(out uint shortDword).Should().BeFalse();
        CreateEntry(REG_VALUE_TYPE.REG_DWORD, new byte[sizeof(uint) + 1])
            .TryReadUInt32(out uint longDword).Should().BeFalse();
        CreateEntry(REG_VALUE_TYPE.REG_QWORD, new byte[sizeof(ulong) - 1])
            .TryReadUInt64(out ulong shortQword).Should().BeFalse();
        CreateEntry(REG_VALUE_TYPE.REG_QWORD, new byte[sizeof(ulong) + 1])
            .TryReadUInt64(out ulong longQword).Should().BeFalse();

        unread.Should().Be(0);
        wrongType.Should().Be(0);
        shortDword.Should().Be(0);
        longDword.Should().Be(0);
        shortQword.Should().Be(0);
        longQword.Should().Be(0);
    }

    private static RegistryValueEntry CreateEntry(
        REG_VALUE_TYPE type,
        ReadOnlySpan<byte> data,
        bool hasData = true)
        => new(
            default,
            keyRelativePath: [],
            name: [],
            depth: 0,
            type,
            data.Length,
            hasData,
            data);

    private static string[] ReadStrings(REG_VALUE_TYPE type, byte[] data, bool hasData = true)
    {
        RegistryValueEntry entry = CreateEntry(type, data, hasData);
        List<string> values = [];

        foreach (ReadOnlySpan<char> value in entry.Strings)
        {
            values.Add(value.ToString());
        }

        return [.. values];
    }

    private static byte[] Utf16(string value) => Encoding.Unicode.GetBytes(value);
}