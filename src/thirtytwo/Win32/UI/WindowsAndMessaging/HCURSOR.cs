// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
///  Cursor handle wrapper.
/// </summary>
public unsafe partial struct HCURSOR : IDisposable
{
    /// <summary>
    ///  Gets the sentinel cursor value represented by <c>-1</c>.
    /// </summary>
    public static HCURSOR Invalid => new(-1);

    /// <summary>
    ///  Loads a cursor resource by identifier.
    /// </summary>
    /// <param name="id">System cursor resource identifier.</param>
    /// <returns>The loaded cursor handle.</returns>
    public static implicit operator HCURSOR(CursorId id) => PInvoke.LoadCursor(default, (PCWSTR)(char*)(uint)id);

    /// <summary>
    ///  Sets this cursor and returns a scope that restores the previous cursor on disposal.
    /// </summary>
    /// <returns>A scope that restores the prior cursor value.</returns>
    public SetScope SetCursorScope() => new(this);

    /// <summary>
    ///  Sets this cursor as the current cursor.
    /// </summary>
    /// <returns>The previously active cursor handle returned by <c>SetCursor</c>.</returns>
    public HCURSOR SetCursor() => PInvoke.SetCursor(this);

    /// <summary>
    ///  Destroys the cursor when the handle is non-null and clears this instance.
    /// </summary>
    /// <remarks>
    ///  This method always calls <c>DestroyCursor</c> for non-null values, so callers must only
    ///  dispose handles they intend to destroy.
    /// </remarks>
    public void Dispose()
    {
        if (!IsNull)
        {
            PInvoke.DestroyCursor(this);
        }

        Unsafe.AsRef(in this) = default;
    }
}