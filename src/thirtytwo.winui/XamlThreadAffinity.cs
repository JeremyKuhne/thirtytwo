// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;

namespace Windows.WinUI;

/// <summary>
///  Captures a XAML object's owner thread and rejects access from other threads.
/// </summary>
internal sealed class XamlThreadAffinity
{
    private readonly Thread _thread = Thread.CurrentThread;

    /// <summary>Captures the current thread as the owner.</summary>
    internal XamlThreadAffinity()
    {
        NativeThreadId = PInvoke.GetCurrentThreadId();
    }

    /// <summary>Gets the managed identifier of the owner thread.</summary>
    internal int ManagedThreadId => _thread.ManagedThreadId;

    /// <summary>Gets the native identifier of the owner thread.</summary>
    internal uint NativeThreadId { get; }

    /// <summary>Verifies that the calling thread is the captured owner thread.</summary>
    /// <exception cref="InvalidOperationException">The calling thread is not the owner thread.</exception>
    internal void VerifyAccess()
    {
        if (ReferenceEquals(Thread.CurrentThread, _thread))
        {
            return;
        }

        int actualManagedThreadId = Environment.CurrentManagedThreadId;
        uint actualNativeThreadId = PInvoke.GetCurrentThreadId();
        throw new InvalidOperationException(
            $"The calling thread does not own this XAML state. Expected managed thread {ManagedThreadId} and native thread {NativeThreadId}; actual managed thread {actualManagedThreadId} and native thread {actualNativeThreadId}.");
    }
}