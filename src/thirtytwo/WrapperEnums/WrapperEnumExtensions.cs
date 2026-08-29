// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Provides extension methods for wrapper enums.
/// </summary>
public static class WrapperEnumExtensions
{
    /// <summary>
    ///  Sets the current cursor to the specified predefined cursor and restores the previous cursor when disposed.
    /// </summary>
    /// <param name="cursor">The predefined cursor identifier to set.</param>
    /// <returns>A scope that restores the previous cursor when disposed.</returns>
    public static HCURSOR.SetScope SetCursorScope(this CursorId cursor) => ((HCURSOR)cursor).SetCursorScope();

    /// <summary>
    ///  Sets the current cursor to the specified predefined cursor.
    /// </summary>
    /// <param name="cursor">The predefined cursor identifier to set.</param>
    /// <returns>The previously active cursor.</returns>
    public static HCURSOR SetCursor(this CursorId cursor) => ((HCURSOR)cursor).SetCursor();
}