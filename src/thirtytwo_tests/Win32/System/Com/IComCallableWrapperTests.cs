// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

public unsafe class IComCallableWrapperTests
{
    [Fact]
    public void IComCallableWrapper_Guid()
    {
        Guid iidGuid = IComCallableWrapper.IID_Guid;
        Guid attributeGuid = typeof(IComCallableWrapper.Interface).GUID;
        attributeGuid.Should().Be(iidGuid);

        Guid iComIIDGuid = *IID.Get<IComCallableWrapper>();
        iComIIDGuid.Should().Be(iidGuid);
    }
}
