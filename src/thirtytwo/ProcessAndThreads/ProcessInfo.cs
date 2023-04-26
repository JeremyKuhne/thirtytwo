// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.System.WindowsProgramming;

namespace Windows.ProcessAndThreads;

public unsafe sealed class ProcessInfo
{
    private readonly byte[] _buffer;
    private readonly int _count;

    // Cache last index to optimize forward searching
    private int _lastProcessIndex;
    private SYSTEM_PROCESS_INFORMATION* _lastProcess;

    public ProcessInfo()
    {
        // On the dev box where I wrote this there were 247 active processes and it needed 512K for the buffer.
        // (Windows 11: two VS instances, Edge with a number of tabs, calc, and notepad)
        _buffer = GC.AllocateUninitializedArray<byte>(1024 * 1024, pinned: true);

        while (true)
        {
            uint length;
            NTSTATUS status = Interop.NtQuerySystemInformation(
                SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                First,
                (uint)_buffer.Length,
                &length);

            if (status == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
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

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ProcessInfo _info;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ProcessInfo info)
        {
            _info = info;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _info.Count)
            {
                _index = index;
                return true;
            }

            return false;
        }

        public ref readonly SYSTEM_PROCESS_INFORMATION Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _info[_index];
        }
    }

    private SYSTEM_PROCESS_INFORMATION* First
        => (SYSTEM_PROCESS_INFORMATION*)Unsafe.AsPointer(ref Unsafe.AsRef(_buffer[0]));

    public int Count => _count;

    public ref SYSTEM_PROCESS_INFORMATION this[int i]
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