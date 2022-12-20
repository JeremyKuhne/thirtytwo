// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

public unsafe partial class ActiveXControl : Control
{
    private static readonly WindowClass s_class = new(
        className: "ActiveXHostControlClass",
        backgroundBrush: HBRUSH.Invalid);

    private readonly Guid _classId;
    private readonly AgileComPointer<IUnknown> _instance;
    private readonly AgileComPointer<IOleInPlaceActiveObject>? _instanceAsActiveObject;
    private readonly Container _container;
    private readonly Site _site;
    private readonly OLEMISC _status;

    public ActiveXControl(
        Guid classId,
        Rectangle bounds,
        Window parentWindow,
        nint parameters = 0) : base(
            bounds,
            parentWindow: parentWindow,
            style: WindowStyles.Child,
            windowClass: s_class,
            parameters: parameters)
    {
        _classId = classId;
        IUnknown* unknown = CreateInstance();
        _instance = new(unknown);
        _instanceAsActiveObject = unknown->QueryAgileInterface<IOleInPlaceActiveObject>();

        using ComScope<IOleObject> oleObject = ComScope<IOleObject>.QueryFrom(unknown);
        if (oleObject.Value->GetMiscStatus(DVASPECT.DVASPECT_CONTENT, out OLEMISC status).Succeeded)
        {
            _status = status;
        }

        _container = new Container(parentWindow.Handle);
        _site = new Site(this);

        IOleClientSite* site = ComHelpers.GetComPointer<IOleClientSite>(_site);
        HRESULT hr = oleObject.Value->SetClientSite(site);

        DoVerb(OLEIVERB.OLEIVERB_INPLACEACTIVATE);
    }

    private IUnknown* CreateInstance()
    {
        IUnknown* unknown = CreateWithIClassFactory2();
        if (unknown is null)
        {
            fixed (Guid* g = &_classId)
            {
                HRESULT hr = Interop.CoCreateInstance(g, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.Get<IUnknown>(), (void**)&unknown);
                hr.ThrowOnFailure();
            }
        }

        return unknown;

        IUnknown* CreateWithIClassFactory2()
        {
            using ComScope<IClassFactory2> factory = new(null);

            fixed (Guid* g = &_classId)
            {
                HRESULT hr = Interop.CoGetClassObject(g, CLSCTX.CLSCTX_INPROC_SERVER, null, IID.Get<IClassFactory2>(), factory);

                if (hr.Failed)
                {
                    Debug.Assert(hr == HRESULT.E_NOINTERFACE);
                    return null;
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
                return (IUnknown*)unknown;
            }
            else
            {
                factory.Value->CreateInstance(null, IID.GetRef<IUnknown>(), out void* unknown);
                return (IUnknown*)unknown;
            }
        }
    }

    protected internal override bool PreProcessMessage(ref MSG message)
    {
        // Give the OCX a chance to translate any keyboard messages.
        var activeObject = _container.ActiveObject ?? _instanceAsActiveObject;
        if (activeObject is null)
        {
            return base.PreProcessMessage(ref message);
        }

        using var scope = activeObject.GetInterface();
        fixed (MSG* msg = &message)
        {
            if (scope.Value->TranslateAccelerator(msg) == HRESULT.S_OK)
            {
                return true;
            }
        }

        return base.PreProcessMessage(ref message);
    }

    private void DoVerb(OLEIVERB verb)
    {
        // The ActiveX Control should be sited with IOleObject::SetClientSite() before doing verbs.
        // https://learn.microsoft.com/windows/win32/api/oleidl/nf-oleidl-ioleobject-doverb#notes-to-callers

        using var oleObject = _instance.GetInterface<IOleObject>();
        RECT bounds = this.GetClientRectangle();
        IOleClientSite* clientSite = ComHelpers.GetComPointer<IOleClientSite>(_site);

        HRESULT hr = oleObject.Value->DoVerb(
            iVerb: (int)verb,
            lpmsg: (MSG*)null,
            pActiveSite: clientSite,
            lindex: 0,
            hwndParent: this.GetParent(),
            lprcPosRect: &bounds);
    }

    /// <summary>
    ///  Fired when an ActiveX Control property changes.
    /// </summary>
    /// <param name="dispatchId">
    ///  The DISPID of the changed property or <see cref="Interop.DISPID_UNKNOWN"/> if multiple properties have
    ///  changed.
    /// </param>
    protected virtual void OnPropertyChanged(int dispatchId)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.WindowPositionChanged:
                // The control will ask us for bounds and update appropriately (via IOleInPlaceSite.GetWindowContext)
                DoVerb(OLEIVERB.OLEIVERB_SHOW);
                return (LRESULT)0;
            case MessageType.Paint:
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _instance.Dispose();
        }

        base.Dispose(disposing);
    }
}