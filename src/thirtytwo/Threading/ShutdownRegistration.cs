// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents a callback registered for dispatcher shutdown.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="Dispatcher.RegisterShutdownCallback"/> adds the callback and returns this handle. Disposing the handle
///   before shutdown begins removes that registration. Once shutdown begins, the callback set is fixed and disposal
///   has no effect. Repeated disposal is safe. A non-default registration must be disposed on its owning thread.
///  </para>
/// </remarks>
public readonly struct ShutdownRegistration : IDisposable
{
    private readonly ThreadContext? _context;
    private readonly long _id;

    /// <summary>
    ///  Creates a handle for a shutdown callback that the thread context has already registered.
    /// </summary>
    /// <param name="context">The thread context that owns the callback.</param>
    /// <param name="id">The registration identifier.</param>
    internal ShutdownRegistration(ThreadContext context, long id)
    {
        _context = context;
        _id = id;
    }

    /// <summary>
    ///  Removes the represented callback registration when shutdown has not begun. Disposal from a different thread
    ///  throws; repeated disposal has no effect.
    /// </summary>
    public void Dispose() => _context?.RemoveShutdownCallback(_id);
}
