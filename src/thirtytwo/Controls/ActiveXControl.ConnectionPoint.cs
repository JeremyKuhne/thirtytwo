// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows;

public unsafe partial class ActiveXControl
{
    private sealed class ConnectionPoint<TSink> : IDisposable
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
            if (container.Value->FindConnectionPoint(IID.Get<TSink>(), &connectionPoint).Failed)
            {
                return;
            }

            _connectionPoint = new(connectionPoint, sink);
        }

        public void Dispose() => _connectionPoint?.Dispose();

        private class ConnectionHandle : AgileComPointer<IConnectionPoint>
        {
            private readonly uint _cookie;
            private readonly bool _connected;

            public ConnectionHandle(IConnectionPoint* connectionPoint, IManagedWrapper sink)
                : base(connectionPoint, takeOwnership: true)
            {
                uint cookie = 0;
                IUnknown* ccw = ComHelpers.TryGetComPointer<IUnknown>(sink, out HRESULT hr);
                if (hr.Failed || connectionPoint->Advise(ccw, &cookie).Failed)
                {
                    Dispose();
                }
                else
                {
                    _connected = true;
                }

                _cookie = cookie;
            }

            protected override void Dispose(bool disposing)
            {
                if (_connected)
                {
                    using var connectionPoint = TryGetInterface(out HRESULT hr);
                    if (hr.Succeeded)
                    {
                        hr = connectionPoint.Value->Unadvise(_cookie);
                    }
                }

                base.Dispose(disposing);
            }
        }
    }
}