// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Based on https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms.Primitives/src/Windows/Win32/Foundation/IID.cs
//
// Original header
// ---------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Foundation;

internal static unsafe class IID
{
    // We cast away the "readonly" here as there is no way to communicate that through a pointer and
    // Marshal APIs take the Guid as ref. Even though none of our usages actually change the state.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid* Get<T>() where T : unmanaged, IComIID
    {
        return (Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in T.Guid));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Guid GetRef<T>() where T : unmanaged, IComIID
    {
        return ref Unsafe.AsRef(in T.Guid);
    }
}
