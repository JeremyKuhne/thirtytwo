// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public partial struct HCURSOR
{
    public readonly struct SetScope : IDisposable
    {
        private readonly HCURSOR _previousCursor;
        public SetScope(HCURSOR cursor) => _previousCursor = PInvoke.SetCursor(cursor);
        public readonly void Dispose() => PInvoke.SetCursor(_previousCursor);
    }
}