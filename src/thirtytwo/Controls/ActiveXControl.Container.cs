// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

public unsafe partial class ActiveXControl
{
    internal sealed unsafe partial class Container(Window containerWindow) : IOleContainer.Interface, IOleInPlaceFrame.Interface, IDisposable,
        // IOleContainer : IParseDisplayName  -   IOleInPlaceFrame : IOleInPlaceUIWindow : IOleWindow
        IManagedWrapper<IOleContainer, IParseDisplayName, IOleInPlaceFrame, IOleInPlaceUIWindow, IOleWindow>
    {
        internal Window Window { get; } = containerWindow;

        public AgileComPointer<IOleInPlaceActiveObject>? ActiveObject { get; private set; }

        HRESULT IOleContainer.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
        {
            if (ppmkOut is not null)
            {
                *ppmkOut = null;
            }

            // From the 1995 control container guidelines, needed:
            //
            //   Only if linking to controls or other embeddings in the container is supported, as this is necessary
            //   for moniker binding.
            //
            // WinForms does not implement this.

            return HRESULT.E_NOTIMPL;
        }

        HRESULT IOleContainer.Interface.EnumObjects(uint grfFlags, IEnumUnknown** ppenum)
        {
            if (ppenum is null)
            {
                return HRESULT.E_POINTER;
            }

            List<ActiveXControl>? activeXControls = null;
            if (((OLECONTF)grfFlags).HasFlag(OLECONTF.OLECONTF_EMBEDDINGS) && Window is not null)
            {
                activeXControls = [];
                Window.EnumerateChildWindows((HWND child) =>
                {
                    if (FromHandle(child) is ActiveXControl control)
                    {
                        activeXControls.Add(control);
                    }

                    return true;
                });
            }

            *ppenum = ComHelpers.TryGetComPointer<IEnumUnknown>(new ActiveXControlEnum(activeXControls), out HRESULT hr);
            return hr;
        }

        // Not needed, see ParseDisplayName comments.
        HRESULT IOleContainer.Interface.LockContainer(BOOL fLock) => HRESULT.E_NOTIMPL;

        HRESULT IParseDisplayName.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
            => ((IOleContainer.Interface)this).ParseDisplayName(pbc, pszDisplayName, pchEaten, ppmkOut);

        HRESULT IOleInPlaceFrame.Interface.GetWindow(HWND* phwnd)
        {
            if (phwnd is null)
            {
                return HRESULT.E_POINTER;
            }

            *phwnd = Window.Handle;
            return HRESULT.S_OK;
        }

        HRESULT IOleInPlaceFrame.Interface.SetActiveObject(IOleInPlaceActiveObject* pActiveObject, PCWSTR pszObjName)
        {
            if (ActiveObject is { } existing)
            {
                if (existing.Equals(pActiveObject))
                {
                    // No need to recreate our agile reference.
                    return HRESULT.S_OK;
                }

                ActiveObject.Dispose();
            }

            // We need the object to call TranslateAccelerator on for incoming key messages.
            ActiveObject = pActiveObject is null ? null : new(pActiveObject, takeOwnership: true);
            return HRESULT.S_OK;
        }

        // These IOleInPlaceFrame interfaces are not required per 1995 OLE Control Container Guidelines. None are
        // supported by WinForms.
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

        public void Dispose() => ActiveObject?.Dispose();
    }
}