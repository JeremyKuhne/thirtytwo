// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

public unsafe partial class ActiveXControl
{
    internal unsafe class OleContainer : IOleContainer.Interface, IOleInPlaceFrame.Interface,
        IManagedWrapper<IOleContainer, IParseDisplayName, IOleInPlaceFrame, IOleInPlaceUIWindow, IOleWindow>
    {
        private readonly HWND _parentWindow;
        private IOleInPlaceActiveObject* _activeObject;

        public OleContainer(HWND parentWindow)
        {
            _parentWindow = parentWindow;
        }

        HRESULT IOleContainer.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
        {
            if (ppmkOut is not null)
            {
                *ppmkOut = null;
            }

            return HRESULT.E_NOTIMPL;
        }

        HRESULT IOleContainer.Interface.EnumObjects(OLECONTF grfFlags, IEnumUnknown** ppenum)
        {
            if (ppenum is null)
            {
                return HRESULT.E_POINTER;
            }

            List<ActiveXControl>? activeXControls = null;
            if (grfFlags.HasFlag(OLECONTF.OLECONTF_EMBEDDINGS) && !_parentWindow.IsNull)
            {
                activeXControls = new();
                _parentWindow.EnumerateChildWindows((HWND child) =>
                {
                    if (FromHandle(child) is ActiveXControl control)
                    {
                        activeXControls.Add(control);
                    }

                    return true;
                });
            }

            *ppenum = Com.TryGetComPointer<IEnumUnknown>(new ActiveXControlEnum(activeXControls), out HRESULT hr);
            return hr;
        }

        private class ActiveXControlEnum : EnumUnknown
        {
            private readonly IReadOnlyList<ActiveXControl>? _controls;

            public ActiveXControlEnum(IReadOnlyList<ActiveXControl>? controls)
                : base(controls?.Count ?? 0)
            {
                _controls = controls;
            }

            protected override EnumUnknown Clone() => new ActiveXControlEnum(_controls);

            protected override IUnknown* GetAtIndex(int index)
            {
                // The caller is responsible for releasing the IUnknown
                IUnknown* unknown = _controls is null ? null : _controls[index]._instance.GetInterface<IUnknown>();
                Debug.Assert(unknown is not null || _controls is null);
                return unknown;
            }
        }

        HRESULT IOleContainer.Interface.LockContainer(BOOL fLock) => HRESULT.E_NOTIMPL;

        HRESULT IParseDisplayName.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
            => ((IOleContainer.Interface)this).ParseDisplayName(pbc, pszDisplayName, pchEaten, ppmkOut);

        HRESULT IOleInPlaceFrame.Interface.GetWindow(HWND* phwnd)
        {
            if (phwnd is null)
            {
                return HRESULT.E_POINTER;
            }

            *phwnd = _parentWindow;
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceFrame.Interface.SetActiveObject(IOleInPlaceActiveObject* pActiveObject, PCWSTR pszObjName)
        {
            _activeObject = pActiveObject;
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceFrame.Interface.ContextSensitiveHelp(BOOL fEnterMode) => HRESULT.S_OK;
        HRESULT IOleInPlaceFrame.Interface.GetBorder(RECT* lprectBorder) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.RequestBorderSpace(RECT* pborderwidths) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.SetBorderSpace(RECT* pborderwidths) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.InsertMenus(HMENU hmenuShared, OLEMENUGROUPWIDTHS* lpMenuWidths) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.SetMenu(HMENU hmenuShared, nint holemenu, HWND hwndActiveObject) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.RemoveMenus(HMENU hmenuShared) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.SetStatusText(PCWSTR pszStatusText) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.EnableModeless(BOOL fEnable) => HRESULT.E_NOTIMPL;
        HRESULT IOleInPlaceFrame.Interface.TranslateAccelerator(MSG* lpmsg, ushort wID) => HRESULT.S_FALSE;

        HRESULT IOleInPlaceUIWindow.Interface.GetWindow(HWND* phwnd)
            => ((IOleInPlaceFrame.Interface)this).GetWindow(phwnd);
        HRESULT IOleInPlaceUIWindow.Interface.ContextSensitiveHelp(BOOL fEnterMode)
            => ((IOleInPlaceFrame.Interface)this).ContextSensitiveHelp(fEnterMode);
        HRESULT IOleInPlaceUIWindow.Interface.GetBorder(RECT* lprectBorder)
            => ((IOleInPlaceFrame.Interface)this).GetBorder(lprectBorder);
        HRESULT IOleInPlaceUIWindow.Interface.RequestBorderSpace(RECT* pborderwidths)
            => ((IOleInPlaceFrame.Interface)this).RequestBorderSpace(pborderwidths);
        HRESULT IOleInPlaceUIWindow.Interface.SetBorderSpace(RECT* pborderwidths)
            => ((IOleInPlaceFrame.Interface)this).SetBorderSpace(pborderwidths);
        HRESULT IOleInPlaceUIWindow.Interface.SetActiveObject(IOleInPlaceActiveObject* pActiveObject, PCWSTR pszObjName)
            => ((IOleInPlaceFrame.Interface)this).SetActiveObject(pActiveObject, pszObjName);
        HRESULT IOleWindow.Interface.GetWindow(HWND* phwnd) => ((IOleInPlaceFrame.Interface)this).GetWindow(phwnd);
        HRESULT IOleWindow.Interface.ContextSensitiveHelp(BOOL fEnterMode)
            => ((IOleInPlaceFrame.Interface)this).ContextSensitiveHelp(fEnterMode);
    }
}