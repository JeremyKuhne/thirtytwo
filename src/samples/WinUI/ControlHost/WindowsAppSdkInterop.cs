// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal static unsafe partial class WindowsAppSdkInterop
{
    [LibraryImport("Microsoft.UI.Windowing.Core.dll")]
    internal static partial int ContentPreTranslateMessage(MSG* message);
}