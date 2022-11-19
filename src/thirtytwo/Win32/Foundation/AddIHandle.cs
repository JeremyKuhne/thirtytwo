// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Foundation;

public partial struct HINSTANCE : IHandle<HINSTANCE>
{
    HINSTANCE IHandle<HINSTANCE>.Handle => this;
    object? IHandle<HINSTANCE>.Wrapper => null;
}