// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Associates a shutdown callback with its registration identifier.
/// </summary>
/// <param name="Id">The registration identifier.</param>
/// <param name="Callback">The registered shutdown callback.</param>
internal sealed record ShutdownCallbackEntry(long Id, Action Callback);
