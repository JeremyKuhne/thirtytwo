// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.System.SystemServices;
using static Windows.Win32.ComExtensions;

namespace Windows;

internal abstract unsafe class DragSource : IDisposable, IDropSource.Interface, IManagedWrapper<IDropSource>
{
    private Window? _attachedWindow;
    private bool _dragging;

    protected DragSource(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.VerifyDropTargetAccess();
        ObjectDisposedException.ThrowIf(window.Handle.IsNull, window);
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("OLE drag-and-drop requires the window's owning thread to be STA.");
        }

        window.AttachDragSource(this);
        _attachedWindow = window;
    }

    HRESULT IDropSource.Interface.QueryContinueDrag(BOOL escapePressed, MODIFIERKEYS_FLAGS keyState)
    {
        if (escapePressed)
        {
            return PInvoke.DRAGDROP_S_CANCEL;
        }

        return (keyState & MODIFIERKEYS_FLAGS.MK_LBUTTON) == 0
            ? PInvoke.DRAGDROP_S_DROP
            : HRESULT.S_OK;
    }

    HRESULT IDropSource.Interface.GiveFeedback(DROPEFFECT effect)
        => PInvoke.DRAGDROP_S_USEDEFAULTCURSORS;

    public void Dispose()
    {
        Window? window = _attachedWindow;
        if (window is null)
        {
            return;
        }

        window.VerifyDropTargetAccess();
        DetachFromWindow(throwOnFailure: true);
    }

    internal void DetachFromWindow(bool throwOnFailure)
    {
        Window? window = _attachedWindow;
        if (window is null)
        {
            return;
        }

        if (throwOnFailure && _dragging)
        {
            throw new InvalidOperationException("The drag source cannot be disposed while a drag operation is active.");
        }

        OnDetaching();
        _attachedWindow = null;
        window.DetachDragSource(this);
    }

    protected virtual void OnDetaching()
    {
    }

    internal DragDropEffects StartDrag(string text)
    {
        if (text.Length == 0)
        {
            return DragDropEffects.None;
        }

        Window window = _attachedWindow ?? throw new ObjectDisposedException(nameof(DragSource));
        window.VerifyDropTargetAccess();
        if (_dragging)
        {
            throw new InvalidOperationException("A drag operation is already active for this source.");
        }

        PInvoke.OleInitialize(null).ThrowOnFailure();
        _dragging = true;
        try
        {
            UnicodeTextDataObject dataObject = new(text);
            using ComScope<IDataObject> dataObjectPointer = new(dataObject.GetComPointer<IDataObject>());
            using ComScope<IDropSource> sourcePointer = new(this.GetComPointer<IDropSource>());
            DROPEFFECT effect = DROPEFFECT.DROPEFFECT_NONE;
            HRESULT result = PInvoke.DoDragDrop(
                dataObjectPointer.Pointer,
                sourcePointer.Pointer,
                DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE,
                &effect);
            result.ThrowOnFailure();
            return result == PInvoke.DRAGDROP_S_DROP
                ? (DragDropEffects)(uint)effect
                : DragDropEffects.None;
        }
        finally
        {
            _dragging = false;
            PInvoke.OleUninitialize();
        }
    }
}
