// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Foundation;

public ref partial struct BstrBuffer
{
    /// <summary>
    ///  Inline storage block for the first <see cref="StackSpace"/> <see cref="BSTR"/> elements.
    /// </summary>
    [InlineArray(StackSpace)]
    private struct StackBuffer
    {
        /// <summary>
        ///  The first element in the inline array backing storage.
        /// </summary>
        internal BSTR _element0;
    }
}