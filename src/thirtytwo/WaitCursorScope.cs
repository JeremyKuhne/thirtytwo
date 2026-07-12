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
        _cursor = PInvoke.SetCursor(PInvoke.LoadCursor(default, PInvoke.IDC_WAIT));
        _ = PInvoke.ShowCursor(true);
    }

    public void Dispose()
    {
        PInvoke.SetCursor(_cursor);
        _ = PInvoke.ShowCursor(false);
    }
}