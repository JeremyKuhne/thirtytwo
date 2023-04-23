// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.System.WindowsProgramming;

namespace Windows.ProcessAndThreads;

public unsafe sealed class ProcessInfo
{
    private readonly byte[] _buffer;
    private readonly int _count;

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
                BufferPointer,
                (uint)_buffer.Length,
                &length);

            if (status == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
            {
                _buffer = GC.AllocateUninitializedArray<byte>((int)length, pinned: true);
                continue;
            }

            status.ThrowIfFailed();

            SYSTEM_PROCESS_INFORMATION* info = (SYSTEM_PROCESS_INFORMATION*)BufferPointer;
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

    private byte* BufferPointer => (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(_buffer[0]));

    public int Count => _count;

    public ref SYSTEM_PROCESS_INFORMATION this[int i]
    {
        get
        {
            if (i < 0 || i >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            fixed (byte* b = _buffer)
            {
                SYSTEM_PROCESS_INFORMATION* info = (SYSTEM_PROCESS_INFORMATION*)b;
                while (i > 0)
                {
                    i--;
                    uint nextOffset = info->NextEntryOffset;
                    if (info->NextEntryOffset == 0)
                    {
                        throw new InvalidOperationException();
                    }

                    info = (SYSTEM_PROCESS_INFORMATION*)((byte*)info + nextOffset);
                }

                return ref Unsafe.AsRef<SYSTEM_PROCESS_INFORMATION>(info);
            }
        }
    }
}