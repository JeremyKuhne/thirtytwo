// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public static unsafe class IUnknownExtensions
{
    extension(ref IUnknown unknown)
    {
        public TInterface* TryQueryInterface<TInterface>() where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = default;
            unknown.QueryInterface(IID.Get<TInterface>(), (void**)&@interface);
            return @interface;
        }

        public TInterface* QueryInterface<TInterface>() where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = default;
            unknown.QueryInterface(IID.Get<TInterface>(), (void**)&@interface).ThrowOnFailure();
            return @interface;
        }

        public AgileComPointer<TInterface>? TryQueryAgileInterface<TInterface>()
            where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = unknown.TryQueryInterface<TInterface>();
            return @interface is null ? null : new(@interface, takeOwnership: true);
        }
    }
}