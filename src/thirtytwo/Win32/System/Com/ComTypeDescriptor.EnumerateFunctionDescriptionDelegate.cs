// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

internal unsafe sealed partial class ComTypeDescriptor
{
    private delegate void EnumerateFunctionDescriptionDelegate(
        ITypeInfo* typeInfo,
        FUNCDESC* function,
        ReadOnlySpan<BSTR> names);
}