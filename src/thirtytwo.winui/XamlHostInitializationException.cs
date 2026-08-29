// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.WinUI;

/// <summary>
///  Reports a WinUI host initialization failure with process and thread context.
/// </summary>
/// <remarks>
///  <para>
///   This exception captures the initialization stage and execution context so failures can be diagnosed across
///   thread-validation, dispatcher-queue, application, and XAML-manager setup.
///  </para>
/// </remarks>
public sealed class XamlHostInitializationException : InvalidOperationException
{
    /// <summary>
    ///  Initializes an exception describing a failed WinUI host initialization stage.
    /// </summary>
    /// <param name="stage">The initialization stage that failed.</param>
    /// <param name="message">The message that describes the failure.</param>
    /// <param name="nativeThreadId">The Win32 identifier of the thread that observed the failure.</param>
    /// <param name="innerException">The exception that caused the initialization failure, if available.</param>
    internal XamlHostInitializationException(
        XamlHostInitializationStage stage,
        string message,
        uint nativeThreadId,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Stage = stage;
        NativeThreadId = nativeThreadId;
        ManagedThreadId = Environment.CurrentManagedThreadId;
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture;
    }

    /// <summary>
    ///  Gets the failed initialization stage.
    /// </summary>
    /// <value>The initialization stage that produced this failure.</value>
    public XamlHostInitializationStage Stage { get; }

    /// <summary>
    ///  Gets the current process architecture.
    /// </summary>
    /// <value>The process architecture at the time the failure was observed.</value>
    public Architecture ProcessArchitecture { get; }

    /// <summary>
    ///  Gets the managed thread identifier that observed the failure.
    /// </summary>
    /// <value>The managed thread identifier captured at exception construction time.</value>
    public int ManagedThreadId { get; }

    /// <summary>
    ///  Gets the native thread identifier that observed the failure.
    /// </summary>
    /// <value>The Win32 thread identifier captured at exception construction time.</value>
    public uint NativeThreadId { get; }
}