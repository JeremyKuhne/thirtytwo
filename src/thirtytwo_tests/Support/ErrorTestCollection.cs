﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.Support;

/// <summary>
///  There are threading issues with <see cref="Marshal.GetExceptionForHR(int)"/>
/// </summary>
[CollectionDefinition(nameof(ErrorTestCollection), DisableParallelization = true)]
public class ErrorTestCollection
{
}