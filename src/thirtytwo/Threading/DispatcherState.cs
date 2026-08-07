// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Defines dispatcher lifecycle states.
/// </summary>
internal enum DispatcherState
{
    /// <summary>The dispatcher has been constructed but not started.</summary>
    Created,

    /// <summary>The dispatcher accepts and processes work.</summary>
    Running,

    /// <summary>The dispatcher no longer accepts work and is shutting down.</summary>
    Stopping,

    /// <summary>The dispatcher has stopped.</summary>
    Stopped,

    /// <summary>The dispatcher stopped because of an infrastructure or unhandled callback failure.</summary>
    Faulted
}
