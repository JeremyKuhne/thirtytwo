﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.Components;

/// <summary>
///  Lighter weight replacement for <see cref="Component"/>.
/// </summary>
[DesignerCategory("Component")]
public class ComponentBase : DisposableBase.Finalizable, IComponent
{
    private event EventHandler? DisposedHandler;
    private readonly object _lock = new();

    ISite? IComponent.Site { get; set; }

    event EventHandler? IComponent.Disposed
    {
        add => DisposedHandler += value;
        remove => DisposedHandler -= value;
    }

    /// <summary>
    ///  Called when the component is being disposed or finalized.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="false"/> if called via a destructor on the finalizer queue. Do not access object fields
    ///  unless <see langword="true"/>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lock)
            {
                ((IComponent)this).Site?.Container?.Remove(this);
                DisposedHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}