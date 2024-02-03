// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe partial struct HCURSOR
{
    public readonly struct SetScope : IDisposable
    {
        private readonly HCURSOR _previousCursor;
        public SetScope(HCURSOR cursor) => _previousCursor = Interop.SetCursor(cursor);
        public readonly void Dispose() => Interop.SetCursor(_previousCursor);
    }
}