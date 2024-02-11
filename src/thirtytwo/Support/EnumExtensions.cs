﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace System;

internal static unsafe class EnumExtensions
{
    // Note that the non-relevant if clauses will be omitted when compiling to a given T.

    /// <summary>
    ///  Returns true if the given flag or flags are set.
    /// </summary>
    /// <remarks>
    ///  Simple wrapper for <see cref="Enum.HasFlag(Enum)"/> that gives you better intellisense.
    /// </remarks>
    public static bool AreFlagsSet<T>(this T value, T flags) where T : unmanaged, Enum => value.HasFlag(flags);

    /// <summary>
    ///  Returns true if only one of the specified flags is set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOnlyOneFlagSet<T>(this T value, T flags) where T : unmanaged, Enum
    {
        // This one doesn't inline without the aggressive inlining hint. Currently this comiles to this for int:
        //
        //  L0000: and ecx, edx
        //  L0002: je short L0010
        //  L0004: blsr eax, ecx
        //  L0009: sete al
        //  L000c: movzx eax, al
        //  L000f: ret
        //  L0010: xor eax, eax
        //  L0012: ret

        if (sizeof(T) == sizeof(byte))
        {
            int v = *(byte*)&value & *(byte*)&flags;
            return v != 0 && (v & (v - 1)) == 0;
        }
        else if (sizeof(T) == sizeof(short))
        {
            int v = *(short*)&value & *(short*)&flags;
            return v != 0 && (v & (v - 1)) == 0;
        }
        else if (sizeof(T) == sizeof(int))
        {
            int v = *(int*)&value & *(int*)&flags;
            return v != 0 && (v & (v - 1)) == 0;
        }
        else if (sizeof(T) == sizeof(long))
        {
            long v = *(long*)&value & *(long*)&flags;
            return v != 0 && (v & (v - 1)) == 0;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    ///  Returns true if any of the given flags are set.
    /// </summary>
    public static bool AreAnyFlagsSet<T>(this T value, T flags) where T : unmanaged, Enum
    {
        if (sizeof(T) == sizeof(byte))
        {
            return (*(byte*)&value & *(byte*)&flags) != 0;
        }
        else if (sizeof(T) == sizeof(short))
        {
            return (*(short*)&value & *(short*)&flags) != 0;
        }
        else if (sizeof(T) == sizeof(int))
        {
            return (*(int*)&value & *(int*)&flags) != 0;
        }
        else if (sizeof(T) == sizeof(long))
        {
            return (*(long*)&value & *(long*)&flags) != 0;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    ///  Sets the given flag or flags.
    /// </summary>
    public static void SetFlags<T>(this ref T value, T flags) where T : unmanaged, Enum
    {
        fixed (T* v = &value)
        {
            // Note that the non-relevant if clauses will be omitted by the JIT so these become one statement.
            if (sizeof(T) == sizeof(byte))
            {
                *(byte*)v |= *(byte*)&flags;
            }
            else if (sizeof(T) == sizeof(short))
            {
                *(short*)v |= *(short*)&flags;
            }
            else if (sizeof(T) == sizeof(int))
            {
                *(int*)v |= *(int*)&flags;
            }
            else if (sizeof(T) == sizeof(long))
            {
                *(long*)v |= *(long*)&flags;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    ///  Clears the given flag or flags.
    /// </summary>
    public static void ClearFlags<T>(this ref T value, T flags) where T : unmanaged, Enum
    {
        fixed (T* v = &value)
        {
            // Note that the non-relevant if clauses will be omitted by the JIT so these become one statement.
            if (sizeof(T) == sizeof(byte))
            {
                *(byte*)v &= (byte)~*(byte*)&flags;
            }
            else if (sizeof(T) == sizeof(short))
            {
                *(short*)v &= (short)~*(short*)&flags;
            }
            else if (sizeof(T) == sizeof(int))
            {
                *(int*)v &= ~*(int*)&flags;
            }
            else if (sizeof(T) == sizeof(long))
            {
                *(long*)v &= ~*(long*)&flags;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}