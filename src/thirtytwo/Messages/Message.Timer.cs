// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>WM_TIMER</c> payload values.
    /// </summary>
    /// <param name="wParam">The timer identifier supplied when the timer was created.</param>
    /// <param name="lParam">The optional timer callback pointer; zero for window-procedure delivery.</param>
    public readonly ref struct Timer(WPARAM wParam, LPARAM lParam)
    {
        /// <summary>
        ///  Gets the timer identifier from <c>wParam</c>.
        /// </summary>
        public uint Id => (uint)wParam;

        /// <summary>
        ///  Gets the native timer callback pointer from <c>lParam</c>, or zero when not provided.
        /// </summary>
        public nint Procedure => lParam;
        // public TimerProcedure? Procedure
        //    => _lParam.IsNull ? null : Marshal.GetDelegateForFunctionPointer<TimerProcedure>(_lParam);
    }
}