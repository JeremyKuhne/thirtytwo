// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Associates a message filter with its registration identifier.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="ThreadContext"/> assigns the next identifier from its per-context sequence when a filter is registered.
///   The returned <see cref="MessageFilterRegistration"/> retains that identifier and uses it to remove this exact
///   entry when disposed.
///  </para>
/// </remarks>
/// <param name="Id">The identifier assigned by the owning thread context when the filter was registered.</param>
/// <param name="Filter">The registered message filter.</param>
internal sealed record MessageFilterEntry(long Id, IMessageFilter Filter);
