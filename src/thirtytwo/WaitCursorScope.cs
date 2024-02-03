// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Temporarily sets the cursor to the wait cursor.
/// </summary>
public readonly ref struct WaitCursorScope
{
    private readonly HCURSOR _cursor;

    public WaitCursorScope()
    {
        _cursor = Interop.SetCursor(Interop.LoadCursor(default, Interop.IDC_WAIT));
        _ = Interop.ShowCursor(true);
    }

    public void Dispose()
    {
        Interop.SetCursor(_cursor);
        _ = Interop.ShowCursor(false);
    }
}