// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Threading;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.System.SystemServices;
using static Windows.Win32.ComExtensions;

namespace Windows;

/// <summary>Attaches OLE drag-and-drop support to a window.</summary>
/// <remarks>
///  <para>
///   Construction initializes OLE and registers the attached window as a drop target. The target is thread-affine and
///   must be disposed on the window's owning STA thread. Window destruction and dispatcher shutdown detach it
///   automatically. Drag data is valid only while its event callback is running.
///  </para>
/// </remarks>
public unsafe class DropTarget : IDisposable, IDropTarget.Interface, IManagedWrapper<IDropTarget>
{
    private const DragDropEffects KnownEffects = DragDropEffects.Copy
        | DragDropEffects.Move
        | DragDropEffects.Link
        | DragDropEffects.Scroll;

    private Window? _attachedWindow;
    private HWND _registeredHandle;
    private ShutdownRegistration _shutdownRegistration;
    private nint _currentDataObject;
    private int _eventDepth;
    private bool _oleInitialized;

    /// <summary>Attaches OLE drag-and-drop support to <paramref name="window"/>.</summary>
    /// <param name="window">The window to register as a drop target.</param>
    public DropTarget(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.VerifyDropTargetAccess();
        ObjectDisposedException.ThrowIf(window.Handle.IsNull, window);
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("OLE drag-and-drop requires the window's owning thread to be STA.");
        }

        window.AttachDropTarget(this);
        _attachedWindow = window;
        try
        {
            PInvoke.OleInitialize(null).ThrowOnFailure();
            _oleInitialized = true;

            if (Dispatcher.Current is { } dispatcher)
            {
                _shutdownRegistration = dispatcher.RegisterShutdownCallback(Dispose);
            }

            RegisterCurrentHandle();
        }
        catch
        {
            DetachFromWindow(throwOnFailure: false);
            throw;
        }
    }

    /// <summary>Occurs when dragged data first enters the attached window.</summary>
    public event EventHandler<DragEventArgs>? DragEnter;

    /// <summary>Occurs while dragged data moves within the attached window.</summary>
    public event EventHandler<DragEventArgs>? DragOver;

    /// <summary>Occurs when dragged data leaves the attached window without being dropped.</summary>
    public event EventHandler? DragLeave;

    /// <summary>Occurs when data is dropped on the attached window.</summary>
    public event EventHandler<DragEventArgs>? DragDrop;

    internal bool IsRaisingEvent => _eventDepth != 0;

    /// <summary>Raises the <see cref="DragEnter"/> event.</summary>
    public virtual void OnDragEnter(DragEventArgs eventArgs)
        => DragEnter?.Invoke(GetAttachedWindow(), eventArgs);

    /// <summary>Raises the <see cref="DragOver"/> event.</summary>
    public virtual void OnDragOver(DragEventArgs eventArgs)
        => DragOver?.Invoke(GetAttachedWindow(), eventArgs);

    /// <summary>Raises the <see cref="DragLeave"/> event.</summary>
    public virtual void OnDragLeave()
        => DragLeave?.Invoke(GetAttachedWindow(), EventArgs.Empty);

    /// <summary>Raises the <see cref="DragDrop"/> event.</summary>
    public virtual void OnDragDrop(DragEventArgs eventArgs)
        => DragDrop?.Invoke(GetAttachedWindow(), eventArgs);

    HRESULT IDropTarget.Interface.DragEnter(
        IDataObject* dataObject,
        MODIFIERKEYS_FLAGS keyState,
        POINTL point,
        DROPEFFECT* effect)
    {
        if (dataObject is null || effect is null)
        {
            return HRESULT.E_POINTER;
        }

        DROPEFFECT allowedEffect = *effect;
        *effect = DROPEFFECT.DROPEFFECT_NONE;
        ReleaseCurrentDataObject();
        dataObject->AddRef();
        _currentDataObject = (nint)dataObject;
        try
        {
            return RaiseDragEvent(
                dataObject,
                keyState,
                point,
                effect,
                allowedEffect,
                static (target, eventArgs) => target.OnDragEnter(eventArgs));
        }
        catch
        {
            try
            {
                OnDragOperationFailed();
            }
            finally
            {
                ReleaseCurrentDataObject();
            }

            throw;
        }
    }

    HRESULT IDropTarget.Interface.DragOver(
        MODIFIERKEYS_FLAGS keyState,
        POINTL point,
        DROPEFFECT* effect)
    {
        if (effect is null)
        {
            return HRESULT.E_POINTER;
        }

        DROPEFFECT allowedEffect = *effect;
        *effect = DROPEFFECT.DROPEFFECT_NONE;
        if (_attachedWindow is null)
        {
            return HRESULT.COR_E_OBJECTDISPOSED;
        }

        IDataObject* dataObject = (IDataObject*)_currentDataObject;
        if (dataObject is null)
        {
            return PInvoke.E_UNEXPECTED;
        }

        try
        {
            return RaiseDragEvent(
                dataObject,
                keyState,
                point,
                effect,
                allowedEffect,
                static (target, eventArgs) => target.OnDragOver(eventArgs));
        }
        catch
        {
            try
            {
                OnDragOperationFailed();
            }
            finally
            {
                ReleaseCurrentDataObject();
            }

            throw;
        }
    }

    HRESULT IDropTarget.Interface.DragLeave()
    {
        try
        {
            if (_attachedWindow is null)
            {
                return HRESULT.COR_E_OBJECTDISPOSED;
            }

            _eventDepth++;
            try
            {
                OnDragLeave();
            }
            finally
            {
                _eventDepth--;
            }

            return HRESULT.S_OK;
        }
        finally
        {
            ReleaseCurrentDataObject();
        }
    }

    HRESULT IDropTarget.Interface.Drop(
        IDataObject* dataObject,
        MODIFIERKEYS_FLAGS keyState,
        POINTL point,
        DROPEFFECT* effect)
    {
        if (dataObject is null || effect is null)
        {
            return HRESULT.E_POINTER;
        }

        DROPEFFECT allowedEffect = *effect;
        *effect = DROPEFFECT.DROPEFFECT_NONE;
        try
        {
            return RaiseDragEvent(
                dataObject,
                keyState,
                point,
                effect,
                allowedEffect,
                static (target, eventArgs) => target.OnDragDrop(eventArgs));
        }
        finally
        {
            ReleaseCurrentDataObject();
        }
    }

    /// <summary>Detaches this target and releases its OLE registration.</summary>
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

    internal bool SuspendForHandleChange()
    {
        Window? window = _attachedWindow;
        if (window is null || _registeredHandle.IsNull)
        {
            return false;
        }

        window.VerifyDropTargetAccess();
        if (IsRaisingEvent)
        {
            throw new InvalidOperationException("The drop-target HWND cannot change while a drag event is being raised.");
        }

        PInvoke.RevokeDragDrop(_registeredHandle).ThrowOnFailure();
        _registeredHandle = default;
        return true;
    }

    internal void ResumeAfterHandleChange()
    {
        Window? window = _attachedWindow;
        if (window is null || !_registeredHandle.IsNull)
        {
            return;
        }

        window.VerifyDropTargetAccess();
        try
        {
            RegisterCurrentHandle();
        }
        catch
        {
            DetachFromWindow(throwOnFailure: false);
            throw;
        }
    }

    internal void DetachFromWindow(bool throwOnFailure)
    {
        Window? window = _attachedWindow;
        if (window is null)
        {
            return;
        }

        if (throwOnFailure && IsRaisingEvent)
        {
            throw new InvalidOperationException("The drop target cannot be disposed while a drag event is being raised.");
        }

        HRESULT result = HRESULT.S_OK;
        if (!_registeredHandle.IsNull)
        {
            result = PInvoke.RevokeDragDrop(_registeredHandle);
            if (result.Failed && throwOnFailure)
            {
                result.ThrowOnFailure();
            }
        }

        _attachedWindow = null;
        _registeredHandle = default;
        window.DetachDropTarget(this);
        ReleaseCurrentDataObject();

        ShutdownRegistration shutdownRegistration = _shutdownRegistration;
        _shutdownRegistration = default;
        bool uninitializeOle = _oleInitialized;
        _oleInitialized = false;
        try
        {
            shutdownRegistration.Dispose();
        }
        finally
        {
            if (uninitializeOle)
            {
                PInvoke.OleUninitialize();
            }
        }

        if (result.Failed)
        {
            Debug.Fail($"RevokeDragDrop failed with {result}.");
        }
    }

    private void RegisterCurrentHandle()
    {
        Window window = GetAttachedWindow();
        using ComScope<IDropTarget> targetPointer = new(this.GetComPointer<IDropTarget>());
        HWND dropTargetHandle = window.GetDropTargetHandleForRegistration();
        if (dropTargetHandle.IsNull)
        {
            throw new InvalidOperationException("The drop-target HWND is unavailable.");
        }

        PInvoke.RegisterDragDrop(dropTargetHandle, targetPointer.Pointer).ThrowOnFailure();
        _registeredHandle = dropTargetHandle;
    }

    private HRESULT RaiseDragEvent(
        IDataObject* dataObject,
        MODIFIERKEYS_FLAGS keyState,
        POINTL point,
        DROPEFFECT* effect,
        DROPEFFECT allowedEffect,
        Action<DropTarget, DragEventArgs> raiseEvent)
    {
        if (_attachedWindow is null)
        {
            return HRESULT.COR_E_OBJECTDISPOSED;
        }

        DragDropEffects projectedAllowedEffect = (DragDropEffects)(uint)allowedEffect;
        DropDataObject data = new(dataObject);
        DragEventArgs eventArgs = new(
            data,
            (DragDropKeyStates)(uint)keyState,
            new Point(point.x, point.y),
            projectedAllowedEffect);

        try
        {
            _eventDepth++;
            raiseEvent(this, eventArgs);
            ValidateEffect(eventArgs.Effect, projectedAllowedEffect);
            *effect = (DROPEFFECT)(uint)eventArgs.Effect;
            return HRESULT.S_OK;
        }
        finally
        {
            _eventDepth--;
            data.Invalidate();
        }
    }

    private Window GetAttachedWindow()
        => _attachedWindow ?? throw new ObjectDisposedException(nameof(DropTarget));

    private protected virtual void OnDragOperationFailed()
    {
    }

    private static void ValidateEffect(DragDropEffects effect, DragDropEffects allowedEffect)
    {
        if ((effect & ~KnownEffects) != 0 || (effect & ~allowedEffect) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(effect),
                effect,
                "The selected drag effect must be permitted by the source.");
        }
    }

    private void ReleaseCurrentDataObject()
    {
        nint dataObject = _currentDataObject;
        _currentDataObject = 0;
        if (dataObject != 0)
        {
            ((IDataObject*)dataObject)->Release();
        }
    }
}
