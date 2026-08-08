// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>
///  Describes the active process XAML host environment.
/// </summary>
/// <param name="OwnerManagedThreadId">The managed owner thread identifier.</param>
/// <param name="OwnerNativeThreadId">The native owner thread identifier.</param>
/// <param name="LeaseCount">The current public lease count.</param>
/// <param name="OwnsApplication">
///  Whether the environment created the process WinUI application instead of adopting an existing one.
/// </param>
/// <param name="OwnsDispatcherQueue">
///  Whether the environment created the current thread's dispatcher queue and will shut it down.
/// </param>
public sealed record XamlHostEnvironmentInfo(
    int OwnerManagedThreadId,
    uint OwnerNativeThreadId,
    int LeaseCount,
    bool OwnsApplication,
    bool OwnsDispatcherQueue);