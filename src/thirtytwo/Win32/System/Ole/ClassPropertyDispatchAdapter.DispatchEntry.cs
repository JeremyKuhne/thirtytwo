// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Ole;

public partial class ClassPropertyDispatchAdapter
{
    /// <summary>
    ///  Dispatch metadata for a projected member.
    /// </summary>
    private struct DispatchEntry
    {
        /// <summary>
        ///  Managed member name used for invocation.
        /// </summary>
        public string Name;

        /// <summary>
        ///  Dispatch property flags reported by <see cref="IDispatchEx.GetMemberProperties(int, uint, FDEX_PROP_FLAGS*)"/>.
        /// </summary>
        public FDEX_PROP_FLAGS Flags;

        /// <summary>
        ///  Dispatch identifier assigned to the member.
        /// </summary>
        public int DispId;
    }
}