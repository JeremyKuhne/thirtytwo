// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.System.WindowsProgramming;

namespace Windows.ProcessAndThreads;

public sealed partial class ProcessInfo
{
    /// <summary>
    ///  Enumerates process records from a <see cref="ProcessInfo"/> snapshot.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ProcessInfo _info;
        private int _index;

        /// <summary>
        ///  Initializes an enumerator over the provided process snapshot.
        /// </summary>
        /// <param name="info">The process snapshot to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ProcessInfo info)
        {
            _info = info;
            _index = -1;
        }

        /// <summary>
        ///  Advances to the next process record.
        /// </summary>
        /// <returns>
        ///  <see langword="true"/> if a next element is available; otherwise, <see langword="false"/>.
        /// </returns>
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

        /// <summary>
        ///  Gets the process record at the current enumerator position.
        /// </summary>
        /// <value>A readonly reference to the current process record.</value>
        public readonly ref readonly SYSTEM_PROCESS_INFORMATION Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _info[_index];
        }
    }
}