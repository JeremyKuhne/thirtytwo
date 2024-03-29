﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Base class for implementing <see cref="IDisposable"/> with double disposal protection.
/// </summary>
public abstract class DisposableBase : IDisposable
{
    private int _disposedValue;

    protected bool Disposed => _disposedValue != 0;

    /// <summary>
    ///  Called when the component is being disposed or finalized.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="false"/> if called via a destructor on the finalizer queue. Do not access object fields
    ///  unless <see langword="true"/>.
    /// </param>
    protected abstract void Dispose(bool disposing);

    private void DisposeInternal(bool disposing)
    {
        // Want to ensure both paths are guarded against double disposal.
        if (Interlocked.Exchange(ref _disposedValue, 1) == 1)
        {
            return;
        }

        Dispose(disposing);
    }

    /// <summary>
    ///  Disposes the component.
    /// </summary>
    public void Dispose()
    {
        DisposeInternal(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  <see cref="DisposableBase"/> with a finalizer.
    /// </summary>
    public abstract class Finalizable : DisposableBase
    {
        ~Finalizable() => DisposeInternal(disposing: false);
    }
}