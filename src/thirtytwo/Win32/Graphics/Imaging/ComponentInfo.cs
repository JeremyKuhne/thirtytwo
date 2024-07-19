// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

public unsafe class ComponentInfo : DirectDrawBase<IWICComponentInfo>
{
    public ComponentInfo(IWICComponentInfo* pointer) : base(pointer) { }

    public ComponentInfo(Guid componentClassId)
        : base(CreateComponentInfo(Application.ImagingFactory, componentClassId))
    {
    }

    public static IWICComponentInfo* CreateComponentInfo(ImagingFactory factory, Guid componentClassId)
    {
        IWICComponentInfo* info;
        factory.Pointer->CreateComponentInfo(&componentClassId, &info).ThrowOnFailure();
        GC.KeepAlive(factory);
        return info;
    }

    public string FriendlyName
    {
        get
        {
            uint length;
            Pointer->GetFriendlyName(0, null, &length).ThrowOnFailure();
            char* name = stackalloc char[(int)length];
            Pointer->GetFriendlyName(length, name, &length).ThrowOnFailure();
            return new string(name);
        }
    }

    public static implicit operator IWICComponentInfo*(ComponentInfo d) => d.Pointer;
}