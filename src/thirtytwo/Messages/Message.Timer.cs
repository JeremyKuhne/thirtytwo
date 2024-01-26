// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref struct Timer(WPARAM wParam, LPARAM lParam)
    {
        public uint Id => (uint)wParam;

        public nint Procedure => lParam;
        // public TimerProcedure? Procedure
        //    => _lParam.IsNull ? null : Marshal.GetDelegateForFunctionPointer<TimerProcedure>(_lParam);
    }
}