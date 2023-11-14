// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows;

public unsafe partial class ActiveXControl
{
    internal sealed unsafe partial class Container
    {
        private class ActiveXControlEnum(IReadOnlyList<ActiveXControl>? controls) : EnumUnknown(controls?.Count ?? 0)
        {
            private readonly IReadOnlyList<ActiveXControl>? _controls = controls;

            protected override EnumUnknown Clone() => new ActiveXControlEnum(_controls);

            protected override IUnknown* GetAtIndex(int index)
            {
                // The caller is responsible for releasing the IUnknown
                IUnknown* unknown = _controls is null ? null : _controls[index]._instance.GetInterface<IUnknown>();
                Debug.Assert(unknown is not null || _controls is null);
                return unknown;
            }
        }
    }
}