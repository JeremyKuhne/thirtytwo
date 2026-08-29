// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Temporarily sets the cursor to the wait cursor.
/// </summary>
public readonly ref struct WaitCursorScope
{
    private readonly HCURSOR _cursor;

    /// <summary>
    ///  Sets the current thread's cursor to the standard wait cursor.
    /// </summary>
    public WaitCursorScope()
    {
        _cursor = PInvoke.SetCursor(PInvoke.LoadCursor(default, PInvoke.IDC_WAIT));
        _ = PInvoke.ShowCursor(true);
    }

    /// <summary>
    ///  Restores the previous cursor captured by this scope.
    /// </summary>
    public void Dispose()
    {
        PInvoke.SetCursor(_cursor);
        _ = PInvoke.ShowCursor(false);
    }
}