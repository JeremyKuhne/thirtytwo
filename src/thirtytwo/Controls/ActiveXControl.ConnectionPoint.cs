// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows;

public unsafe partial class ActiveXControl
{
    private sealed class ConnectionPoint<TSink> : IDisposable
        where TSink : unmanaged, IComIID
    {
        private readonly AgileComPointer<IConnectionPoint>? _connectionPoint;
        private readonly uint _cookie;

        public ConnectionPoint(AgileComPointer<IUnknown> control, IManagedWrapper sink)
        {
            using var container = control.TryGetInterface<IConnectionPointContainer>(out HRESULT hr);
            if (hr.Failed)
            {
                return;
            }

            IConnectionPoint* connectionPoint;
            if (container.Value->FindConnectionPoint(IID.Get<TSink>(), &connectionPoint).Failed)
            {
                return;
            }

            _connectionPoint = new(connectionPoint);

            uint cookie = 0;
            IUnknown* ccw = ComHelpers.TryGetComPointer<IUnknown>(sink, out hr);
            if (hr.Failed || connectionPoint->Advise(ccw, &cookie).Failed)
            {
                _connectionPoint.Dispose();
                _connectionPoint = null;
            }

            _cookie = cookie;
        }

        public void Dispose()
        {
            if (_connectionPoint is null)
            {
                return;
            }

            using var connectionPoint = _connectionPoint.TryGetInterface();
            HRESULT hr = connectionPoint.Value->Unadvise(_cookie);
            _connectionPoint.Dispose();
        }
    }
}