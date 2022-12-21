// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public interface IComPointer : IDisposable
{
    ComScope<TAsInterface> GetInterface<TAsInterface>() where TAsInterface : unmanaged, IComIID;
    ComScope<TAsInterface> TryGetInterface<TAsInterface>(out HRESULT hr) where TAsInterface : unmanaged, IComIID;
}