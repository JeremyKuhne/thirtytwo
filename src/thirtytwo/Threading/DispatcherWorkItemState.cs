// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Defines dispatcher operation states.
/// </summary>
internal enum DispatcherWorkItemState
{
    /// <summary>The operation is waiting to run.</summary>
    Queued,

    /// <summary>The operation callback has started.</summary>
    Running,

    /// <summary>The operation completed successfully.</summary>
    Succeeded,

    /// <summary>The operation was canceled.</summary>
    Canceled,

    /// <summary>The operation completed with an exception.</summary>
    Faulted
}
