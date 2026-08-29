// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Wdk.System.SystemInformation;
using Windows.Win32.System.WindowsProgramming;

namespace Windows.ProcessAndThreads;

/// <summary>
///  Captures a snapshot of process information returned by <c>NtQuerySystemInformation</c>.
/// </summary>
/// <remarks>
///  <para>
///   The snapshot is immutable after construction and backed by a pinned buffer that remains valid for the lifetime
///   of this instance.
///  </para>
/// </remarks>
public sealed unsafe partial class ProcessInfo
{
    private readonly byte[] _buffer;
    private readonly int _count;

    // Cache last index to optimize forward searching
    private int _lastProcessIndex;
    private SYSTEM_PROCESS_INFORMATION* _lastProcess;

    /// <summary>
    ///  Initializes a new process snapshot for the current system.
    /// </summary>
    /// <exception cref="ThirtyTwoException"><c>NtQuerySystemInformation</c> returned a failing status.</exception>
    public ProcessInfo()
    {
        // On the dev box where I wrote this there were 247 active processes and it needed 512K for the buffer.
        // (Windows 11: two VS instances, Edge with a number of tabs, calc, and notepad)
        _buffer = GC.AllocateUninitializedArray<byte>(1024 * 1024, pinned: true);

        while (true)
        {
            uint length;
            NTSTATUS status = Wdk.Interop.NtQuerySystemInformation(
                SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                First,
                (uint)_buffer.Length,
                &length);

            if (status == PInvoke.STATUS_INFO_LENGTH_MISMATCH)
            {
                _buffer = GC.AllocateUninitializedArray<byte>((int)length, pinned: true);
                continue;
            }

            status.ThrowIfFailed();

            SYSTEM_PROCESS_INFORMATION* info = First;
            _lastProcess = info;
            _lastProcessIndex = 0;

            while (true)
            {
                _count++;
                uint nextOffset = info->NextEntryOffset;
                if (info->NextEntryOffset == 0)
                {
                    return;
                }

                info = (SYSTEM_PROCESS_INFORMATION*)((byte*)info + nextOffset);
            }
        }
    }

    /// <summary>
    ///  Returns an enumerator over the snapshot entries.
    /// </summary>
    /// <returns>An enumerator that iterates each process record in order.</returns>
    public Enumerator GetEnumerator() => new(this);

    private SYSTEM_PROCESS_INFORMATION* First
        => (SYSTEM_PROCESS_INFORMATION*)Unsafe.AsPointer(ref Unsafe.AsRef(ref _buffer[0]));

    /// <summary>
    ///  Gets the number of process records in this snapshot.
    /// </summary>
    public int Count => _count;

    /// <summary>
    ///  Gets the process record at the specified index.
    /// </summary>
    /// <param name="i">The zero-based index of the process record.</param>
    /// <returns>A readonly reference to the process information at <paramref name="i"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="i"/> is outside the snapshot bounds.</exception>
    /// <exception cref="InvalidOperationException">The snapshot chain ended unexpectedly.</exception>
    public ref readonly SYSTEM_PROCESS_INFORMATION this[int i]
    {
        get
        {
            if (i < 0 || i >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            if (i == _lastProcessIndex)
            {
                return ref Unsafe.AsRef<SYSTEM_PROCESS_INFORMATION>(_lastProcess);
            }

            SYSTEM_PROCESS_INFORMATION* info;
            int remaining = i;
            if (i < _lastProcessIndex)
            {
                info = First;
            }
            else
            {
                info = _lastProcess;
                remaining -= _lastProcessIndex;
            }

            while (remaining > 0)
            {
                remaining--;
                uint nextOffset = info->NextEntryOffset;
                if (info->NextEntryOffset == 0)
                {
                    throw new InvalidOperationException();
                }

                info = (SYSTEM_PROCESS_INFORMATION*)((byte*)info + nextOffset);
            }

            _lastProcess = info;
            _lastProcessIndex = i;
            return ref Unsafe.AsRef<SYSTEM_PROCESS_INFORMATION>(info);
        }
    }
}