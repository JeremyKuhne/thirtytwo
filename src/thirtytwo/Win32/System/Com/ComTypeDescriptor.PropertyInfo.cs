// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

internal sealed partial class ComTypeDescriptor
{
    private struct PropertyInfo
    {
        public string? Name { get; set; }
        public int DispatchId { get; set; }
        public bool HasSetter { get; set; }
        public VARENUM Type { get; set; }
    }
}