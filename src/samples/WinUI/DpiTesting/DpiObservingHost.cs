// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.WinUI;

namespace DpiTesting;

/// <summary>Exposes the protected host DPI callback to the diagnostic sample.</summary>
internal sealed class DpiObservingHost : XamlHostControl
{
    internal DpiObservingHost(Rectangle bounds, Window parentWindow, Func<DpiTestContent> contentFactory)
        : base(bounds, parentWindow, () => contentFactory())
    {
    }

    internal event Action<uint, uint>? DpiTransition;

    protected override void OnDpiChanged(uint oldDpi, uint newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        DpiTransition?.Invoke(oldDpi, newDpi);
    }
}
