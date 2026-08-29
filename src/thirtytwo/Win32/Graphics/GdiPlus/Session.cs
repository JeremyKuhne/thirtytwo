// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Represents a started GDI+ session.
/// </summary>
/// <remarks>
///  <para>
///   Disposing the session shuts down the associated GDI+ token.
///  </para>
/// </remarks>
public class Session : DisposableBase.Finalizable
{
    private UIntPtr _token;

    /// <summary>
    ///  Starts a new GDI+ session.
    /// </summary>
    /// <param name="version">The GDI+ startup version to request.</param>
    /// <exception cref="Exception">The underlying GDI+ startup call failed.</exception>
    public Session(uint version = 2)
    {
        _token = GdiPlus.Startup(version);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        GdiPlus.Shutdown(_token);
        _token = UIntPtr.Zero;
    }
}