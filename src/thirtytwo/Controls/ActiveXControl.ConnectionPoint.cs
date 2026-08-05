// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows;

public unsafe partial class ActiveXControl
{
    private sealed partial class ConnectionPoint<TSink> : IDisposable
        where TSink : unmanaged, IComIID
    {
        private readonly ConnectionHandle? _connectionPoint;

        public ConnectionPoint(AgileComPointer<IUnknown> control, IManagedWrapper sink)
        {
            using var container = control.TryGetInterface<IConnectionPointContainer>(out HRESULT hr);
            if (hr.Failed)
            {
                return;
            }

            IConnectionPoint* connectionPoint;
            if (container.Pointer->FindConnectionPoint(IID.Get<TSink>(), &connectionPoint).Failed)
            {
                return;
            }

            _connectionPoint = new(connectionPoint, sink);
        }

        public void Dispose() => _connectionPoint?.Dispose();
    }
}