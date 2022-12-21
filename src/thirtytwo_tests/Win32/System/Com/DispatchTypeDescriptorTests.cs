// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32;
using Windows.Win32.System.Com;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ComTypeDescriptorTests
{
    private static readonly Guid s_mediaPlayerClassId = new("6BF52A52-394A-11d3-B153-00C04F79FAA6");

    [StaFact]
    public void ComTypeDescriptor_ClassName_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(ComHelpers.CreateComClass(s_mediaPlayerClassId), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        string? className = descriptor.GetClassName();
        className.Should().Be("WindowsMediaPlayer");
    }

    [StaFact]
    public void ComTypeDescriptor_ComponentName_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(ComHelpers.CreateComClass(s_mediaPlayerClassId), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        string? className = descriptor.GetComponentName();
        className.Should().BeEmpty();
    }

    [StaFact]
    public void ComTypeDescriptor_GetProperties_MediaPlayer()
    {
        using AgileComPointer<IUnknown> unknown = new(ComHelpers.CreateComClass(s_mediaPlayerClassId), takeOwnership: true);
        ComTypeDescriptor comDescriptor = new(unknown);
        ICustomTypeDescriptor descriptor = comDescriptor;
        var properties = descriptor.GetProperties();
        properties.Count.Should().Be(11);
        var urlDescriptor = properties["URL"];
        urlDescriptor.Should().NotBeNull();
        urlDescriptor!.IsReadOnly.Should().BeFalse();
    }
}
