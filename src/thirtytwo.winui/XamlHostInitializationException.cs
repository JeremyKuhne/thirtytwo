// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.WinUI;

/// <summary>
///  Reports a WinUI host initialization failure with process and thread context.
/// </summary>
public sealed class XamlHostInitializationException : InvalidOperationException
{
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

    /// <summary>Gets the failed initialization stage.</summary>
    public XamlHostInitializationStage Stage { get; }

    /// <summary>Gets the current process architecture.</summary>
    public Architecture ProcessArchitecture { get; }

    /// <summary>Gets the managed thread identifier that observed the failure.</summary>
    public int ManagedThreadId { get; }

    /// <summary>Gets the native thread identifier that observed the failure.</summary>
    public uint NativeThreadId { get; }
}