// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.Win32.System.Com;

[TestClass]
public unsafe class ComTypeDescriptorTests
{
    [STATestMethod]
    public void ComTypeDescriptor_ClassName_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(CLSID.WindowsMediaPlayer.CreateComClass(), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        string? className = descriptor.GetClassName();
        className.Should().Be("WindowsMediaPlayer");
    }

    [STATestMethod]
    public void ComTypeDescriptor_ComponentName_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(CLSID.WindowsMediaPlayer.CreateComClass(), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        string? className = descriptor.GetComponentName();
        className.Should().BeEmpty();
    }

    [STATestMethod]
    public void ComTypeDescriptor_GetProperties_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(CLSID.WindowsMediaPlayer.CreateComClass(), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        var properties = descriptor.GetProperties();
        properties.Count.Should().Be(11);
        var urlDescriptor = properties["URL"];
        urlDescriptor.Should().NotBeNull();
        urlDescriptor!.IsReadOnly.Should().BeFalse();
    }

    [STATestMethod]
    public void ComTypeDescriptor_GetEvents_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(CLSID.WindowsMediaPlayer.CreateComClass(), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        var events = descriptor.GetEvents();
        events.Count.Should().Be(39);
        ComEventDescriptor playStateChange = (ComEventDescriptor)events["PlayStateChange"]!;
        playStateChange.Attributes.Count.Should().Be(0);
        playStateChange.EventType.Should().Be(typeof(Action<int>));
        playStateChange.Description.Should().Be("Sent when the control changes PlayState");
        playStateChange.DispatchId.Should().Be(5101);
    }
}
