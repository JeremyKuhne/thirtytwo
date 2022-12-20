// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

public unsafe partial class ActiveXControl
{
    private sealed class Site :
        IOleClientSite.Interface,
        ISimpleFrameSite.Interface,
        IOleInPlaceSite.Interface,
        IOleWindow.Interface,
        IOleControlSite.Interface,
        IPropertyNotifySink.Interface,
        IDisposable,
        IManagedWrapper<IOleClientSite, ISimpleFrameSite, IPropertyNotifySink, IOleInPlaceSite, IOleWindow>
    {
        private readonly ActiveXControl _control;
        private bool _inOnChanged;
        private readonly ConnectionPoint<IPropertyNotifySink> _connectionPoint;

        public Site(ActiveXControl control)
        {
            _control = control;
            _connectionPoint = new(_control._instance, this);
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

            *ppContainer = ComHelpers.GetComPointer<IOleContainer>(_control._container);
            return HRESULT.S_OK;
        }

        HRESULT IOleClientSite.Interface.ShowObject()
        {
            // WinForms has quite a bit of logic here that seems mostly related to design mode and the fact that
            // controls can be delay created.
            return HRESULT.S_OK;
        }

        HRESULT IOleClientSite.Interface.OnShowWindow(BOOL fShow) => HRESULT.S_OK;

        HRESULT IOleClientSite.Interface.RequestNewObjectLayout() => HRESULT.E_NOTIMPL;

        HRESULT IPropertyNotifySink.Interface.OnChanged(int dispID)
        {
            // Getting properties can sometimes fire OnChanged- try to avoid recursion.
            if (_inOnChanged)
            {
                return HRESULT.S_OK;
            }

            _inOnChanged = true;
            try
            {
                _control.OnPropertyChanged(dispID);
            }
            finally
            {
                _inOnChanged = false;
            }

            return HRESULT.S_OK;
        }

        HRESULT IPropertyNotifySink.Interface.OnRequestEdit(int dispID)
        {
            // This callback is used to force a readonly state by returning S_FALSE. For now just allow everything.
            return HRESULT.S_OK;
        }

        // Both of the ISimpleFrameSite messages can return E_NOTIMPL to say we don't do message filtering.
        // WinForms returns S_OK and S_FALSE, perhaps some controls depend on it?

        HRESULT ISimpleFrameSite.Interface.PreMessageFilter(
            HWND hWnd,
            uint msg,
            WPARAM wp,
            LPARAM lp,
            LRESULT* plResult,
            uint* pdwCookie)
            // Continue processing the message.
            => HRESULT.S_OK;

        HRESULT ISimpleFrameSite.Interface.PostMessageFilter(
            HWND hWnd,
            uint msg,
            WPARAM wp,
            LPARAM lp,
            LRESULT* plResult,
            uint dwCookie)
            // We did not process the message.
            => HRESULT.S_FALSE;

        public void Dispose()
        {
            _connectionPoint.Dispose();
        }

        HRESULT IOleInPlaceSite.Interface.GetWindow(HWND* phwnd) => ((IOleWindow.Interface)this).GetWindow(phwnd);
        HRESULT IOleInPlaceSite.Interface.ContextSensitiveHelp(BOOL fEnterMode)
            => ((IOleWindow.Interface)this).ContextSensitiveHelp(fEnterMode);

        HRESULT IOleInPlaceSite.Interface.CanInPlaceActivate() => HRESULT.S_OK;

        HRESULT IOleInPlaceSite.Interface.OnInPlaceActivate()
        {
            // WinForms sets state as OC_INPLACE
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceSite.Interface.OnUIActivate()
        {
            // WinForms sets state as OC_UIACTIVE and calls Container.OnUIActivate to update the AxHost and
            // attach SelectionChanging if sited with an ISelectionService.
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceSite.Interface.GetWindowContext(
            IOleInPlaceFrame** ppFrame,
            IOleInPlaceUIWindow** ppDoc,
            RECT* lprcPosRect,
            RECT* lprcClipRect,
            OLEINPLACEFRAMEINFO* lpFrameInfo)
        {
            if (ppFrame is null || ppDoc is null || lprcPosRect is null || lprcClipRect is null || lpFrameInfo is null)
            {
                // Docs say all pointers must be set to null if an error occurs.
                ppFrame = null;
                ppDoc = null;
                lpFrameInfo = null;
                lprcPosRect = null;
                lprcClipRect = null;
                lpFrameInfo = null;
                return HRESULT.E_POINTER;
            }

            *ppFrame = ComHelpers.GetComPointer<IOleInPlaceFrame>(_control._container);
            *ppDoc = null;

            *lprcPosRect = _control.GetClientRectangle();
            *lprcClipRect = new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
            lpFrameInfo->cb = (uint)sizeof(OLEINPLACEFRAMEINFO);
            lpFrameInfo->fMDIApp = false;
            lpFrameInfo->haccel = HACCEL.Null;
            lpFrameInfo->hwndFrame = _control.GetParent();

            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceSite.Interface.Scroll(SIZE scrollExtant) => HRESULT.S_FALSE;

        HRESULT IOleInPlaceSite.Interface.OnUIDeactivate(BOOL fUndoable)
        {
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceSite.Interface.OnInPlaceDeactivate()
        {
            // WinForms detaches the container and destroys the Window for the container. Mid 90s optimization maybe?
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceSite.Interface.DiscardUndoState() => HRESULT.S_OK;

        HRESULT IOleInPlaceSite.Interface.DeactivateAndUndo()
        {
            using var inPlace = _control._instance.GetInterface<IOleInPlaceObject>();
            return inPlace.Value->UIDeactivate();
        }

        HRESULT IOleInPlaceSite.Interface.OnPosRectChange(RECT* lprcPosRect)
        {
            if (lprcPosRect is null)
            {
                return HRESULT.E_POINTER;
            }

            using var inPlace = _control._instance.GetInterface<IOleInPlaceObject>();
            RECT clipRect = new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
            HRESULT hr = inPlace.Value->SetObjectRects(lprcPosRect, &clipRect);
            return HRESULT.S_OK;
        }

        HRESULT IOleWindow.Interface.GetWindow(HWND* phwnd)
        {
            if (phwnd is null)
            {
                return HRESULT.E_POINTER;
            }

            *phwnd = _control.GetParent();
            return HRESULT.S_OK;
        }

        HRESULT IOleWindow.Interface.ContextSensitiveHelp(BOOL fEnterMode) => HRESULT.E_NOTIMPL;
        HRESULT IOleControlSite.Interface.OnControlInfoChanged() => HRESULT.S_OK;
        HRESULT IOleControlSite.Interface.LockInPlaceActive(BOOL fLock) => HRESULT.E_NOTIMPL;
        HRESULT IOleControlSite.Interface.GetExtendedControl(IDispatch** ppDisp) => HRESULT.E_NOTIMPL;

        HRESULT IOleControlSite.Interface.TransformCoords(POINTL* pPtlHimetric, PointF* pPtfContainer, XFORMCOORDS dwFlags)
        {
            if (pPtlHimetric is null || pPtfContainer is null)
            {
                return HRESULT.E_POINTER;
            }

            if (dwFlags.HasFlag(XFORMCOORDS.XFORMCOORDS_HIMETRICTOCONTAINER))
            {
                *pPtfContainer = new(_control.HiMetricToPixel(pPtlHimetric->x), _control.HiMetricToPixel(pPtlHimetric->y));
            }
            else if (dwFlags.HasFlag(XFORMCOORDS.XFORMCOORDS_CONTAINERTOHIMETRIC))
            {
                *pPtlHimetric = new()
                {
                    x = _control.PixelToHiMetric((int)pPtfContainer->X),
                    y = _control.PixelToHiMetric((int)pPtfContainer->Y)
                };
            }

            return HRESULT.S_OK;
        }

        HRESULT IOleControlSite.Interface.TranslateAccelerator(MSG* pMsg, KEYMODIFIERS grfModifiers)
        {
            if (pMsg == null)
            {
                return HRESULT.E_POINTER;
            }

            // We didn't process the message.
            return HRESULT.S_FALSE;
        }

        HRESULT IOleControlSite.Interface.OnFocus(BOOL fGotFocus) => HRESULT.S_OK;
        HRESULT IOleControlSite.Interface.ShowPropertyFrame() => HRESULT.E_NOTIMPL;
    }
}