// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Windows.Win32.UI.WindowsAndMessaging;

public class IconTests
{
    [Fact]
    public void GetFileIconCount_Regedit()
    {
        HICON.GetFileIconCount("regedit.exe").Should().BeOneOf(5, 7);
    }

    [Fact]
    public void GetFileIconCount_OneDrive()
    {
        if (File.Exists(@"C:\Windows\System32\OneDrive.ico"))
        {
            HICON.GetFileIconCount(@"C:\Windows\System32\OneDrive.ico").Should().Be(1);
        }
    }

    [Fact]
    public void ExtractIcon_File()
    {
        using HICON icon = HICON.ExtractIcon("regedit.exe", 1);
        Size size = icon.GetSize();
        size.Should().NotBe(Size.Empty);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(15, 15)]
    [InlineData(300, 300)]
    public void ExtractIcon_StockIcon_Sizes(int requestedSize, int expectedSize)
    {
        using HICON icon = HICON.ExtractIcon(SHSTOCKICONID.SIID_APPLICATION, size: (ushort)requestedSize);
        Size size = icon.GetSize();
        size.Should().Be(new Size(expectedSize, expectedSize));
    }

    [Fact]
    public void ExtractIcon_Draw()
    {
        using HICON icon = HICON.ExtractIcon(SHSTOCKICONID.SIID_DEVICECAMERA, size: 256);
        using IconTestWindow window = new(icon);
        Application.Run(window);
    }

    public class IconTestWindow : Window
    {
        private readonly HICON _icon;

        public IconTestWindow(HICON icon)
            : base(DefaultBounds, style: WindowStyles.OverlappedWindow, windowClass: new(backgroundBrush: SystemColor.Shadow3d))
            => _icon = icon;

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Paint:
                    {
                        using var deviceContext = window.BeginPaint();
                        deviceContext.DrawIcon(_icon, default);
                        window.PostMessage(MessageType.Quit);
                        break;
                    }
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
