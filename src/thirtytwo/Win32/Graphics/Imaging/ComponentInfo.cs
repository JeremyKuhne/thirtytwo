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
            using BufferScope<char> name = new(stackalloc char[256]);
            name.EnsureCapacity(checked((int)length));
            fixed (char* namePointer = name)
            {
                Pointer->GetFriendlyName((uint)name.Length, namePointer, &length).ThrowOnFailure();
                int characterCount = checked((int)length);
                if (characterCount == 0 || name[characterCount - 1] != '\0')
                {
                    throw new InvalidDataException("The WIC component returned an invalid friendly name.");
                }

                return name[..(characterCount - 1)].ToString();
            }
        }
    }

    public static implicit operator IWICComponentInfo*(ComponentInfo d) => d.Pointer;
}