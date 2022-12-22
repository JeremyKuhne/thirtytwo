// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
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
    private readonly ICustomTypeDescriptor _typeDescriptor;
    private readonly Window _parentWindow;
    private PropertyDescriptorCollection? _propertyDescriptors;
    private bool _activated;
    private bool _shown;

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
        IUnknown* unknown = ComHelpers.CreateComClass(classId);
        _instance = new(unknown, takeOwnership: true);
        _instanceAsActiveObject = unknown->QueryAgileInterface<IOleInPlaceActiveObject>();

        using ComScope<IOleObject> oleObject = ComScope<IOleObject>.QueryFrom(unknown);
        if (oleObject.Value->GetMiscStatus(DVASPECT.DVASPECT_CONTENT, out OLEMISC status).Succeeded)
        {
            _status = status;
        }

        _parentWindow = parentWindow;
        _container = new Container(parentWindow);
        _site = new Site(this);

        IOleClientSite* site = ComHelpers.GetComPointer<IOleClientSite>(_site);
        HRESULT hr = oleObject.Value->SetClientSite(site);

        _typeDescriptor = new ComTypeDescriptor(_instance);
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

    /// <summary>
    ///  Send the desired <paramref name="verb"/>.
    /// </summary>
    /// <param name="bounds">Bounds of the control in parent client pixel coordinates.</param>
    private void DoVerb(OLEIVERB verb, Rectangle bounds)
    {
        // The ActiveX Control should be sited with IOleObject::SetClientSite() before doing verbs.
        // https://learn.microsoft.com/windows/win32/api/oleidl/nf-oleidl-ioleobject-doverb#notes-to-callers

        using var oleObject = _instance.GetInterface<IOleObject>();

        // Bounds are in pixels here, not HIMETRIC
        RECT rect = bounds;
        IOleClientSite* clientSite = ComHelpers.GetComPointer<IOleClientSite>(_site);

        HRESULT hr = oleObject.Value->DoVerb(
            iVerb: (int)verb,
            lpmsg: (MSG*)null,
            pActiveSite: clientSite,
            lindex: 0,
            hwndParent: _parentWindow,
            lprcPosRect: &rect);
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
                PositionChanged();
                return (LRESULT)0;
            case MessageType.Paint:
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);

        void PositionChanged()
        {
            Rectangle bounds = this.GetClientRectangle();
            Rectangle boundsInParent = this.MapTo(_parentWindow, bounds);

            if (!_activated)
            {
                // Some controls only query size on activation, so we wait until the initial resize comes through.
                DoVerb(OLEIVERB.OLEIVERB_INPLACEACTIVATE, boundsInParent);
                _activated = true;
            }

            if (_status.HasFlag(OLEMISC.OLEMISC_RECOMPOSEONRESIZE))
            {
                // Control wants notified.
                using var oleObject = _instance.TryGetInterface<IOleObject>(out HRESULT hr);
                if (hr.Succeeded)
                {
                    // Not specifically called out in the SetExtent docs, but OLE defaults are HIMETRic
                    Size size = PixelToHiMetric(this.GetClientRectangle().Size);
                    oleObject.Value->SetExtent(DVASPECT.DVASPECT_CONTENT, (SIZE*)&size);
                }
            }

            using (var inPlaceObject = _instance.TryGetInterface<IOleInPlaceObject>(out HRESULT hr))
            {
                // Coordinates here are in pixels, not HIMETRIC
                if (hr.Succeeded)
                {
                    RECT rect = boundsInParent;
                    inPlaceObject.Value->SetObjectRects(&rect, &rect);
                }
            }

            if (!_shown)
            {
                // Some controls, but not all, ask for bounds on show (via IOleInPlaceSite.GetWindowContext)
                DoVerb(OLEIVERB.OLEIVERB_SHOW, boundsInParent);
                _shown = true;
            }
        }
    }

    protected PropertyDescriptorCollection ComPropertyDescriptors
        => _propertyDescriptors ??= _typeDescriptor.GetProperties();

    protected void SetComProperty(string name, object? value)
        => ComPropertyDescriptors[name]!.SetValue(_instance, value);

    protected object? GetComProperty(string name)
        => ComPropertyDescriptors[name]!.GetValue(_instance);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _instance.Dispose();
        }

        base.Dispose(disposing);
    }
}