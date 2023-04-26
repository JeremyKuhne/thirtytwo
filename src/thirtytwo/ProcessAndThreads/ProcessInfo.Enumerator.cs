// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.System.WindowsProgramming;

namespace Windows.ProcessAndThreads;

public sealed unsafe partial class ProcessInfo
{
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

        public readonly ref readonly SYSTEM_PROCESS_INFORMATION Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _info[_index];
        }
    }
}