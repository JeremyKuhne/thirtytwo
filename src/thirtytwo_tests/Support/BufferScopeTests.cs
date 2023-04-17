// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Tests.Windows.Support;

public unsafe class BufferScopeTests
{
    [Fact]
    public void Construct_WithStackAlloc()
    {
        using BufferScope<char> buffer = new(stackalloc char[10]);
        buffer.Length.Should().Be(10);
        buffer[0] = 'Y';
        buffer[..1].ToString().Should().Be("Y");
    }

    [Fact]
    public void Construct_WithStackAlloc_GrowAndCopy()
    {
        using BufferScope<char> buffer = new(stackalloc char[10]);
        buffer.Length.Should().Be(10);
        buffer[0] = 'Y';
        buffer.EnsureCapacity(64, copy: true);
        buffer.Length.Should().BeGreaterThanOrEqualTo(64);
        buffer[..1].ToString().Should().Be("Y");
    }

    [Fact]
    public void Construct_WithStackAlloc_Pin()
    {
        using BufferScope<char> buffer = new(stackalloc char[10]);
        buffer.Length.Should().Be(10);
        buffer[0] = 'Y';
        fixed (char* c = buffer)
        {
            (*c).Should().Be('Y');
            *c = 'Z';
        }

        buffer[..1].ToString().Should().Be("Z");
    }

    [Fact]
    public void Construct_GrowAndCopy()
    {
        using BufferScope<char> buffer = new(32);
        buffer.Length.Should().BeGreaterThanOrEqualTo(32);
        buffer[0] = 'Y';
        buffer.EnsureCapacity(64, copy: true);
        buffer.Length.Should().BeGreaterThanOrEqualTo(64);
        buffer[..1].ToString().Should().Be("Y");
    }

    [Fact]
    public void Construct_Pin()
    {
        using BufferScope<char> buffer = new(64);
        buffer.Length.Should().BeGreaterThanOrEqualTo(64);
        buffer[0] = 'Y';
        fixed (char* c = buffer)
        {
            (*c).Should().Be('Y');
            *c = 'Z';
        }

        buffer[..1].ToString().Should().Be("Z");
    }
}
