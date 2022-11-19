// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe partial struct PCWSTR
{
    public bool IsNull => Value is null;
}
