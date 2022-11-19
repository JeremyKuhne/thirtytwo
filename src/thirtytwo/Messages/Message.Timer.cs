// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows;

public static partial class Message
{
    public readonly ref struct Timer
    {
        private readonly WPARAM _wParam;
        private readonly LPARAM _lParam;

        public Timer(WPARAM wParam, LPARAM lParam)
        {
            _wParam = wParam;
            _lParam = lParam;
        }

        public uint Id => (uint)_wParam;
        //public TimerProcedure? Procedure
        //    => _lParam.IsNull ? null : Marshal.GetDelegateForFunctionPointer<TimerProcedure>(_lParam);
    }
}