// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Threading;

namespace Windows;

/// <summary>
///  Represents a message filter registered with a UI thread.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="Application.AddMessageFilter"/> adds the filter and returns this handle. Disposing the handle removes
///   that registration. This follows the same pattern as <see cref="CancellationTokenRegistration"/>.
///  </para>
/// </remarks>
public readonly struct MessageFilterRegistration : IDisposable
{
    private readonly ThreadContext? _context;
    private readonly long _id;

    /// <summary>
    ///  Creates a handle for a filter that the thread context has already registered.
    /// </summary>
    /// <param name="context">The thread context that owns the filter.</param>
    /// <param name="id">The registration identifier.</param>
    internal MessageFilterRegistration(ThreadContext context, long id)
    {
        _context = context;
        _id = id;
    }

    /// <summary>
    ///  Removes the represented filter registration. Disposal from a different thread throws.
    /// </summary>
    public void Dispose() => _context?.RemoveMessageFilter(_id);
}
