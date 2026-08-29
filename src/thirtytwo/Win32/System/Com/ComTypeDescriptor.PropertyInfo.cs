// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

internal sealed partial class ComTypeDescriptor
{
    /// <summary>
    ///  Stores intermediate COM property metadata while merging getter and setter entries.
    /// </summary>
    private struct PropertyInfo
    {
        /// <summary>
        ///  Gets or sets the COM property name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///  Gets or sets the COM dispatch identifier.
        /// </summary>
        public int DispatchId { get; set; }

        /// <summary>
        ///  Gets or sets whether a COM property setter is available.
        /// </summary>
        public bool HasSetter { get; set; }

        /// <summary>
        ///  Gets or sets the COM VARIANT type for the property.
        /// </summary>
        public VARENUM Type { get; set; }
    }
}