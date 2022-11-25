// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public readonly ref struct ActivationScope
{
    private readonly nuint _cookie;

    internal ActivationScope(ActivationContext? context)
    {
        _cookie = context?.Activate() ?? 0;
    }

    public void Dispose()
    {
        if (_cookie != 0)
        {
            ActivationContext.Deactivate(_cookie);
        }
    }
}