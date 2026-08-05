// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows;

public unsafe partial class ActiveXControl
{
    private sealed partial class ConnectionPoint<TSink>
        where TSink : unmanaged, IComIID
    {
        private class ConnectionHandle : AgileComPointer<IConnectionPoint>
        {
            private readonly uint _cookie;
            private readonly bool _connected;

            public ConnectionHandle(IConnectionPoint* connectionPoint, IManagedWrapper sink)
                : base(connectionPoint, takeOwnership: true)
            {
                uint cookie = 0;
                using ComScope<IUnknown> ccw = new(sink.TryGetComPointer<IUnknown>(out HRESULT hr));
                if (hr.Failed || connectionPoint->Advise(ccw.Pointer, &cookie).Failed)
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
                        hr = connectionPoint.Pointer->Unadvise(_cookie);
                    }
                }

                base.Dispose(disposing);
            }
        }
    }
}