// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public delegate void BufferAction<T>(ref ValueBuffer<T> buffer)
    where T : unmanaged;