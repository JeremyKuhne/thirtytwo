using System.Runtime.InteropServices;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MinimalWinUIHost;

internal static unsafe partial class WindowsAppSdkInterop
{
    [LibraryImport("Microsoft.UI.Windowing.Core.dll", EntryPoint = "ContentPreTranslateMessage")]
    internal static partial int ContentPreTranslateMessage(MSG* message);
}
