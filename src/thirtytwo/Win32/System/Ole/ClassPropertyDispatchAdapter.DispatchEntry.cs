// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Ole;

public partial class ClassPropertyDispatchAdapter
{
    private struct DispatchEntry
    {
        public string Name;
        public FDEX_PROP_FLAGS Flags;
        public int DispId;
    }
}