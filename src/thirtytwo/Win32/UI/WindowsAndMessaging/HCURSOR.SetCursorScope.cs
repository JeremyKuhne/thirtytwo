// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public partial struct HCURSOR
{
    /// <summary>
    ///  Scope that restores the previously active cursor when disposed.
    /// </summary>
    public readonly struct SetScope : IDisposable
    {
        /// <summary>
        ///  Cursor that was active before the scope set a new cursor.
        /// </summary>
        private readonly HCURSOR _previousCursor;

        /// <summary>
        ///  Sets the provided cursor and captures the prior cursor for restoration.
        /// </summary>
        /// <param name="cursor">Cursor to set for the lifetime of the scope.</param>
        public SetScope(HCURSOR cursor) => _previousCursor = PInvoke.SetCursor(cursor);

        /// <summary>
        ///  Restores the cursor that was active before this scope was created.
        /// </summary>
        public readonly void Dispose() => PInvoke.SetCursor(_previousCursor);
    }
}