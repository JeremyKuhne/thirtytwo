// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI.IntegrationHarness;

internal static class ScreenshotCapture
{
    private const long MaximumPixelCount = 64 * 1024 * 1024;

    internal static ScreenshotSnapshot Capture(long windowHandle, int expectedProcessId, string outputPath)
    {
        if (windowHandle == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowHandle));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        HWND window = WindowHandleValidation.Validate(windowHandle, expectedProcessId);
        if (!PInvoke.GetWindowRect(window, out RECT bounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        int width = checked(bounds.right - bounds.left);
        int height = checked(bounds.bottom - bounds.top);
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException($"Window bounds must be positive, but were {width}x{height}.");
        }

        long pixelCount = checked((long)width * height);
        if (pixelCount > MaximumPixelCount)
        {
            throw new InvalidOperationException($"Window bounds contain {pixelCount:N0} pixels, exceeding the capture limit.");
        }

        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using System.Drawing.Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(
                bounds.left,
                bounds.top,
                0,
                0,
                new Size(width, height),
                CopyPixelOperation.SourceCopy);
        }

        bitmap.Save(outputPath, ImageFormat.Png);
        return new(outputPath, width, height, CountSampledColors(bitmap));
    }

    private static int CountSampledColors(System.Drawing.Bitmap bitmap)
    {
        int horizontalStep = Math.Max(1, bitmap.Width / 64);
        int verticalStep = Math.Max(1, bitmap.Height / 64);
        HashSet<int> colors = [];

        for (int y = 0; y < bitmap.Height; y += verticalStep)
        {
            for (int x = 0; x < bitmap.Width; x += horizontalStep)
            {
                colors.Add(bitmap.GetPixel(x, y).ToArgb());
            }
        }

        return colors.Count;
    }
}
