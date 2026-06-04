// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Run tests in parallel at the method level.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
