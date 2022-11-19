// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HGDIOBJ
{
    public OBJ_TYPE GetObjectType() => (OBJ_TYPE)Interop.GetObjectType(this);
}
