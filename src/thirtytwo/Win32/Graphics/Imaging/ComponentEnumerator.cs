// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Imaging;

public unsafe class ComponentEnumerator : DirectDrawBase<IEnumUnknown>
{
    public ComponentEnumerator(IEnumUnknown* pointer) : base(pointer) { }

    public ComponentEnumerator(WICComponentType componentType)
        : base(CreateComponentEnumerator(Application.ImagingFactory, componentType))
    {
    }

    public static IEnumUnknown* CreateComponentEnumerator(ImagingFactory factory, WICComponentType componentType)
    {
        IEnumUnknown* enumerator;
        factory.Pointer->CreateComponentEnumerator(
            (uint)componentType,
            (uint)WICComponentEnumerateOptions.WICComponentEnumerateDefault,
            &enumerator).ThrowOnFailure();

        GC.KeepAlive(factory);
        return enumerator;
    }

    public static implicit operator IEnumUnknown*(ComponentEnumerator d) => d.Pointer;

    public bool Next([NotNullWhen(true)] out ComponentInfo? componentInfo)
    {
        componentInfo = null;
        IEnumUnknown* enumerator = this;
        if (enumerator is null)
        {
            return false;
        }

        uint fetched;
        using ComScope<IUnknown> unknown = new(null);
        HRESULT result = enumerator->Next(1, unknown, &fetched);
        if (result != HRESULT.S_OK || fetched != 1)
        {
            return false;
        }

        componentInfo = new(unknown.Pointer->QueryInterface<IWICComponentInfo>());
        return true;
    }
}