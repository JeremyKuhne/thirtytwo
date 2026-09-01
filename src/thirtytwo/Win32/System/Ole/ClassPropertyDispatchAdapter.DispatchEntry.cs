// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Windows.Win32.System.Ole;

public partial class ClassPropertyDispatchAdapter
{
    /// <summary>
    ///  Dispatch metadata for a projected member.
    /// </summary>
    private struct DispatchEntry
    {
        /// <summary>
        ///  Managed member name exposed through dispatch.
        /// </summary>
        public string Name;

        /// <summary>
        ///  Reflected property used for invocation.
        /// </summary>
        public PropertyInfo Property;

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