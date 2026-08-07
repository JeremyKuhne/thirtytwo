// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Provides a fire-and-forget dispatcher exception and allows the UI thread to mark it handled.
/// </summary>
/// <param name="exception">The exception raised by dispatched work.</param>
public sealed class DispatcherUnhandledExceptionEventArgs(Exception exception) : EventArgs
{
    /// <summary>
    ///  Gets the exception raised by dispatched work.
    /// </summary>
    public Exception Exception { get; } = exception;

    /// <summary>
    ///  Gets or sets whether the dispatcher should continue pumping messages.
    /// </summary>
    public bool Handled { get; set; }
}
