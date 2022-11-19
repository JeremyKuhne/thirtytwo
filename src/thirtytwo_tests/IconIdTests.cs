// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Win32;

namespace Tests.Windows;

public unsafe class IconIdTests
{
    [Fact]
    public void IconId_Defines()
    {
        IconId.Application.Should().Be((IconId)(uint)Interop.IDI_APPLICATION.Value);
        IconId.Asterisk.Should().Be((IconId)(uint)Interop.IDI_ASTERISK.Value);
        IconId.Error.Should().Be((IconId)Interop.IDI_ERROR);
        IconId.Exclamation.Should().Be((IconId)(uint)Interop.IDI_EXCLAMATION.Value);
        IconId.Hand.Should().Be((IconId)(uint)Interop.IDI_HAND.Value);
        IconId.Information.Should().Be((IconId)Interop.IDI_INFORMATION);
        IconId.Question.Should().Be((IconId)(uint)Interop.IDI_QUESTION.Value);
        IconId.Shield.Should().Be((IconId)(uint)Interop.IDI_SHIELD.Value);
        IconId.Warning.Should().Be((IconId)Interop.IDI_WARNING);
        IconId.WindowsLogo.Should().Be((IconId)(uint)Interop.IDI_WINLOGO.Value);
    }
}
