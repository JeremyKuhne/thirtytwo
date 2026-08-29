// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI device context handle (<c>HDC</c>).
/// </summary>
public unsafe partial struct HDC : IHandle<HDC>
{
    /// <inheritdoc cref="IHandle{T}.Handle"/>
    HDC IHandle<HDC>.Handle => this;

    /// <inheritdoc cref="IHandle{T}.Wrapper"/>
    object? IHandle<HDC>.Wrapper => null;

    /// <summary>
    ///  Reinterprets a generic GDI object handle as a device context handle.
    /// </summary>
    /// <param name="handle">Source handle expected to be <c>OBJ_DC</c> or null.</param>
    /// <returns>The same native value typed as <see cref="HDC"/>.</returns>
    public static explicit operator HDC(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)PInvoke.GetObjectType(handle) == OBJ_TYPE.OBJ_DC);
        return new(handle.Value);
    }

    /// <summary>
    ///  Reinterprets a device context handle as a generic GDI object handle.
    /// </summary>
    /// <param name="handle">The device context handle.</param>
    /// <returns>The same native value typed as <see cref="HGDIOBJ"/>.</returns>
    public static implicit operator HGDIOBJ(HDC handle) => new(handle.Value);
}