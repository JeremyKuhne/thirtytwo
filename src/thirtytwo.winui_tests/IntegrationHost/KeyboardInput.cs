// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace IntegrationHost;

internal static unsafe class KeyboardInput
{
    internal static void PostKey(HWND target, VirtualKey key, bool keyDown, bool systemKey = false)
    {
        uint scanCode = PInvoke.MapVirtualKey((uint)key, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC);
        nint keyData = 1 | ((nint)scanCode << 16);
        if (systemKey)
        {
            keyData |= 0x20000000;
        }

        if (!keyDown)
        {
            keyData |= unchecked((nint)0xC0000000);
        }

        uint message = (keyDown, systemKey) switch
        {
            (true, true) => Interop.WM_SYSKEYDOWN,
            (false, true) => Interop.WM_SYSKEYUP,
            (true, false) => Interop.WM_KEYDOWN,
            _ => Interop.WM_KEYUP
        };
        if (!PInvoke.PostMessage(target, message, (WPARAM)(nuint)key, (LPARAM)keyData))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    internal static void PostKeyPress(HWND target, VirtualKey key, bool systemKey = false)
    {
        PostKey(target, key, keyDown: true, systemKey);
        PostKey(target, key, keyDown: false, systemKey);
    }

    internal static byte SetKeyState(VirtualKey key, bool pressed)
    {
        int keyIndex = (ushort)key;
        if (keyIndex >= 256)
        {
            throw new ArgumentOutOfRangeException(nameof(key), key, "The virtual-key value must fit in the keyboard-state table.");
        }

        Span<byte> keyboardState = stackalloc byte[256];
        fixed (byte* keyboardStatePointer = keyboardState)
        {
            if (!PInvoke.GetKeyboardState(keyboardStatePointer))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            byte previousState = keyboardState[keyIndex];
            keyboardState[keyIndex] = pressed
                ? (byte)(previousState | 0x80)
                : (byte)(previousState & 0x7F);
            if (!PInvoke.SetKeyboardState(keyboardStatePointer))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            return previousState;
        }
    }

    internal static void RestoreKeyState(VirtualKey key, byte state)
    {
        int keyIndex = (ushort)key;
        if (keyIndex >= 256)
        {
            throw new ArgumentOutOfRangeException(nameof(key), key, "The virtual-key value must fit in the keyboard-state table.");
        }

        Span<byte> keyboardState = stackalloc byte[256];
        fixed (byte* keyboardStatePointer = keyboardState)
        {
            if (!PInvoke.GetKeyboardState(keyboardStatePointer))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            keyboardState[keyIndex] = state;
            if (!PInvoke.SetKeyboardState(keyboardStatePointer))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
        }
    }
}