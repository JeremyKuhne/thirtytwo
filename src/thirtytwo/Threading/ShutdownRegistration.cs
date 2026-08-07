// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents a shutdown callback registered with a thread context.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="ThreadContext.RegisterShutdownCallback"/> adds the callback and returns this handle. Disposing the
///   handle before shutdown begins removes that registration. Once shutdown begins, the callback set is fixed and
///   disposal has no effect.
///  </para>
/// </remarks>
internal readonly struct ShutdownRegistration : IDisposable
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
    ///  throws.
    /// </summary>
    public void Dispose() => _context?.RemoveShutdownCallback(_id);
}
