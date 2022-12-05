// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Implement to get called back after <see cref="CustomComWrappers"/> has generated the <see cref="IUnknown"/>
///  for a managed object.
/// </summary>
internal unsafe interface IWrapperInitialize
{
    void OnInitialized(IUnknown* unknown);
}