// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

internal unsafe sealed partial class ComTypeDescriptor
{
    /// <summary>
    ///  Callback used while enumerating COM function descriptions.
    /// </summary>
    /// <param name="typeInfo">Borrowed <see cref="ITypeInfo"/> pointer for the enumerated member.</param>
    /// <param name="function">Borrowed function description pointer for the current member.</param>
    /// <param name="names">
    ///  Resolved names where index 0 is the member name and remaining entries are parameter names.
    /// </param>
    private delegate void EnumerateFunctionDescriptionDelegate(
        ITypeInfo* typeInfo,
        FUNCDESC* function,
        ReadOnlySpan<BSTR> names);
}