// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32.UI.Accessibility;

public unsafe class UiaProviderForNonClientTests
{
    [StaFact]
    public void UiaProviderForNonClient_Window()
    {
        using Window window = new(Window.DefaultBounds);

        using ComScope<IRawElementProviderSimple> provider = new(null);
        HRESULT hr = Interop.UiaProviderForNonClient(
            window,
            (int)OBJECT_IDENTIFIER.OBJID_WINDOW,
            (int)Interop.CHILDID_SELF,
            provider);

        hr.Succeeded.Should().BeTrue();

        VARIANT variant = default;
        hr = provider.Pointer->GetPropertyValue(UIA_PROPERTY_ID.UIA_BoundingRectanglePropertyId, &variant);
        hr.Succeeded.Should().BeTrue();

        // Not sure why we get no bounding rect.
        variant.IsEmpty.Should().BeTrue();
    }
}
