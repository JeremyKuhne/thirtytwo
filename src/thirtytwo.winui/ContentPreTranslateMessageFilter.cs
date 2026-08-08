// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.WinUI;

/// <summary>
///  Gives the Windows App SDK the first opportunity to preprocess messages retrieved by the UI thread.
/// </summary>
internal sealed unsafe partial class ContentPreTranslateMessageFilter : IMessageFilter
{
    /// <inheritdoc/>
    public bool PreFilterMessage(ref MSG message)
    {
        bool isTabKeyDown = message.message == Interop.WM_KEYDOWN
            && (ushort)message.wParam.Value == (ushort)VirtualKey.Tab;

        bool handledByXaml;
        fixed (MSG* messagePointer = &message)
        {
            handledByXaml = ContentPreTranslateMessage(messagePointer) != 0;
        }

        if (!isTabKeyDown)
        {
            return handledByXaml;
        }

        bool targetsXamlHost = Window.FromHandle(message.hwnd, walkParents: true) is XamlHostControl;
        if (targetsXamlHost && handledByXaml)
        {
            return true;
        }

        bool movedFocus = XamlFocusNavigation.TryMoveFocus(
            message.hwnd,
            forward: !XamlFocusNavigation.IsShiftPressed());
        return movedFocus || handledByXaml;
    }

    // Native BOOL is a 32-bit integer; LibraryImport cannot generate the projected BOOL wrapper in this WinRT assembly.
    [LibraryImport("Microsoft.UI.Windowing.Core.dll", EntryPoint = "ContentPreTranslateMessage")]
    private static partial int ContentPreTranslateMessage(MSG* message);
}