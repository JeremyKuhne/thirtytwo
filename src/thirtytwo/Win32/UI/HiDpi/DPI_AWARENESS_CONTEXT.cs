// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.HiDpi;

public partial struct DPI_AWARENESS_CONTEXT
{
    public bool IsNull => Value == 0;
}
