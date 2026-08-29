// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents a temporary activation of an <see cref="ActivationContext"/> for the current thread.
/// </summary>
/// <remarks>Disposing this scope deactivates the activation cookie when activation succeeded.</remarks>
public readonly ref struct ActivationScope
{
    private readonly nuint _cookie;

    /// <summary>
    ///  Activates the provided context and captures its deactivation cookie.
    /// </summary>
    /// <param name="context">The context to activate, or <see langword="null"/> to create a no-op scope.</param>
    internal ActivationScope(ActivationContext? context)
    {
        _cookie = context?.Activate() ?? 0;
    }

    /// <summary>
    ///  Deactivates the captured activation cookie.
    /// </summary>
    public void Dispose()
    {
        if (_cookie != 0)
        {
            ActivationContext.Deactivate(_cookie);
        }
    }
}