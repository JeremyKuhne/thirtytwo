// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

public class ComponentInfoTests
{
    [Fact]
    public void EnumerateDecoders()
    {
        ComponentEnumerator enumerator = new(WICComponentType.WICDecoder);
        while (enumerator.Next(out ComponentInfo? info))
        {
            Assert.NotNull(info);
            Assert.NotNull(info.FriendlyName);
            info.Dispose();
        }
    }
}
