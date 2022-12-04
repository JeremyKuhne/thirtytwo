// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Drawing;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

public unsafe partial class ActiveXControl : Control
{
    private static readonly WindowClass s_textLabelClass = new(className: "ActiveXHostControlClass");
    private readonly Guid _classId;
    private AgileComPointer<IUnknown> _instance;

    public ActiveXControl(
        Guid classId,
        Rectangle bounds,
        string? text = null,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = null,
        nint parameters = 0) : base(bounds, text, style, extendedStyle, parentWindow, s_textLabelClass, parameters)
    {
        _classId = classId;
        CreateInstance();
    }

    [MemberNotNull(nameof(_instance))]
    private void CreateInstance()
    {
        if (CreateWithIClassFactory2())
        {
            return;
        }

        fixed (Guid* g = &_classId)
        {
            IUnknown* unknown;
            HRESULT hr = Interop.CoCreateInstance(g, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.Get<IUnknown>(), (void**)&unknown);
            hr.ThrowOnFailure();
            _instance = new(unknown);
        }

        [MemberNotNullWhen(true, nameof(_instance))]
        bool CreateWithIClassFactory2()
        {
            using ComScope<IClassFactory2> factory = new(null);

            fixed (Guid* g = &_classId)
            {
                HRESULT hr = Interop.CoGetClassObject(g, CLSCTX.CLSCTX_INPROC_SERVER, null, IID.Get<IClassFactory2>(), factory);

                if (hr.Failed)
                {
                    Debug.Assert(hr == HRESULT.E_NOINTERFACE);
                    return false;
                }
            }

            LICINFO info = new()
            {
                cbLicInfo = sizeof(LICINFO)
            };

            factory.Value->GetLicInfo(&info);
            if (info.fRuntimeKeyAvail)
            {
                using BSTR key = default;
                factory.Value->RequestLicKey(0, &key);
                factory.Value->CreateInstanceLic(null, null, IID.GetRef<IUnknown>(), key, out void* unknown);
                _instance = new((IUnknown*)unknown);
            }
            else
            {
                factory.Value->CreateInstance(null, IID.GetRef<IUnknown>(), out void* unknown);
                _instance = new((IUnknown*)unknown);
            }

            return true;
        }
    }

    private void DoVerb(int verb)
    {
        using var oleObject = _instance.GetInterface<IOleObject>();
        // oleObject.Value->DoVerb(verb, null, )
        verb = 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _instance.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed class OleInterfaces : IOleClientSite.Interface, IDisposable
    {
        private readonly ActiveXControl _control;

        public OleInterfaces(ActiveXControl control)
        {
            _control = control;
        }

        HRESULT IOleClientSite.Interface.SaveObject() => HRESULT.E_NOTIMPL;

        HRESULT IOleClientSite.Interface.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, IMoniker** ppmk)
        {
            if (ppmk is null)
            {
                return HRESULT.E_POINTER;
            }

            *ppmk = null;
            return HRESULT.E_NOTIMPL;
        }

        HRESULT IOleClientSite.Interface.GetContainer(IOleContainer** ppContainer)
        {
            if (ppContainer is null)
            {
                return HRESULT.E_POINTER;
            }

            throw new NotImplementedException();
        }

        HRESULT IOleClientSite.Interface.ShowObject() => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.OnShowWindow(BOOL fShow) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.RequestNewObjectLayout() => throw new NotImplementedException();
        public void Dispose() => throw new NotImplementedException();
    }
}