// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal static unsafe class WindowsAppSdkInterop
{
    [DllImport("Microsoft.UI.Windowing.Core.dll", ExactSpelling = true)]
    internal static extern BOOL ContentPreTranslateMessage(MSG* message);
}
