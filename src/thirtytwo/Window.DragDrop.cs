// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class Window
{
    private static TextDragSession? s_activeTextDragSession;

    private DragSource? _attachedDragSource;
    private DropTarget? _attachedDropTarget;

    internal static TextDragSession? ActiveTextDragSession => Volatile.Read(ref s_activeTextDragSession);

    /// <summary>Gets the HWND used by an attached <see cref="DropTarget"/>.</summary>
    /// <remarks>Derived hosts may return an owned child HWND that receives pointer hit-testing.</remarks>
    protected virtual HWND GetDropTargetHandle() => Handle;

    /// <summary>Gets whether a text drop target is attached.</summary>
    protected bool IsTextDropEnabled => _attachedDropTarget is TextDropTarget;

    /// <summary>Attaches or detaches automatic text-drop handling.</summary>
    protected void SetTextDropEnabled(
        bool value,
        Action<string> replaceSelection,
        Func<(int Start, int End)>? getSelection = null,
        Action<int, int>? setSelection = null,
        Func<Point, int>? getInsertionIndex = null,
        Func<int>? getTextLength = null,
        bool focusForInsertion = false,
        Action? beginInsertion = null,
        Action? cancelInsertion = null,
        Action? commitInsertion = null)
    {
        VerifyDropTargetAccess();
        ObjectDisposedException.ThrowIf(Handle.IsNull, this);
        if (value == IsTextDropEnabled)
        {
            return;
        }

        if (value)
        {
            _ = new TextDropTarget(
                this,
                replaceSelection,
                getSelection,
                setSelection,
                getInsertionIndex,
                getTextLength,
                focusForInsertion,
                beginInsertion,
                cancelInsertion,
                commitInsertion);
        }
        else
        {
            ((TextDropTarget)_attachedDropTarget!).Dispose();
        }
    }

    /// <summary>Gets whether a text drag source is attached.</summary>
    protected bool IsTextDragEnabled => _attachedDragSource is TextDragSource;

    /// <summary>Attaches or detaches automatic text-drag handling.</summary>
    protected void SetTextDragEnabled(
        bool value,
        Func<string> getText,
        Func<long> getTextGeneration,
        Func<(int Start, int End)>? getSelection = null,
        Action<int, int>? setSelection = null,
        Action<string>? replaceSelection = null,
        Func<Point, int>? getCharacterIndex = null,
        Func<Point, bool>? canStartDrag = null,
        bool handleWindowMessages = true)
    {
        VerifyDropTargetAccess();
        ObjectDisposedException.ThrowIf(Handle.IsNull, this);
        if (value == IsTextDragEnabled)
        {
            return;
        }

        if (value)
        {
            _ = new TextDragSource(
                this,
                getText,
                getTextGeneration,
                getSelection,
                setSelection,
                replaceSelection,
                getCharacterIndex,
                canStartDrag,
                handleWindowMessages);
        }
        else
        {
            ((TextDragSource)_attachedDragSource!).Dispose();
        }
    }

    protected internal void BeginTextDragSession(
        int sourceStart,
        int sourceEnd,
        Func<long> getSourceGeneration,
        Func<(int Start, int End)> getSelection,
        Action<int, int> setSelection,
        Action<string> replaceSelection)
    {
        TextDragSession session = new(
            this,
            sourceStart,
            sourceEnd,
            getSourceGeneration,
            getSelection,
            setSelection,
            replaceSelection);
        if (Interlocked.CompareExchange(ref s_activeTextDragSession, session, null) is not null)
        {
            throw new InvalidOperationException("A text drag operation is already active.");
        }
    }

    protected internal void EndTextDragSession(DragDropEffects effect)
    {
        TextDragSession? session = ActiveTextDragSession;
        if (session is null || !session.IsSource(this))
        {
            return;
        }

        try
        {
            session.Complete(effect);
        }
        finally
        {
            _ = Interlocked.CompareExchange(ref s_activeTextDragSession, null, session);
        }
    }

    /// <summary>Gets whether the index is within this window's active text-drag source range.</summary>
    protected bool IsTextDragSourceIndex(int index)
        => ActiveTextDragSession?.ContainsSourceIndex(this, index) == true;

    /// <summary>Records an insertion into this window for its active text-drag transaction.</summary>
    protected void RecordTextDragInsertion(int insertionStart, int insertedLength)
        => ActiveTextDragSession?.RecordSameSourceInsertion(this, insertionStart, insertedLength);

    /// <summary>Revokes an attached drop target before replacing its registration HWND.</summary>
    /// <returns><see langword="true"/> when a registration was suspended.</returns>
    protected bool SuspendDropTargetForHandleChange()
        => _attachedDropTarget?.SuspendForHandleChange() ?? false;

    /// <summary>Registers a suspended drop target with the current <see cref="GetDropTargetHandle"/>.</summary>
    protected void ResumeDropTargetAfterHandleChange()
        => _attachedDropTarget?.ResumeAfterHandleChange();

    /// <summary>Disposes an attached drop target before destroying its registration HWND.</summary>
    protected void DisposeAttachedDropTarget()
    {
        if (_attachedDropTarget is { } target)
        {
            target.Dispose();
        }
    }

    internal void AttachDropTarget(DropTarget target)
    {
        if (_attachedDropTarget is not null)
        {
            throw new InvalidOperationException("The window already has an attached drop target.");
        }

        _attachedDropTarget = target;
    }

    internal void AttachDragSource(DragSource source)
    {
        if (_attachedDragSource is not null)
        {
            throw new InvalidOperationException("The window already has an attached drag source.");
        }

        _attachedDragSource = source;
    }

    internal void DetachDragSource(DragSource source)
    {
        if (ReferenceEquals(_attachedDragSource, source))
        {
            _attachedDragSource = null;
        }
    }

    internal void DetachDropTarget(DropTarget target)
    {
        if (ReferenceEquals(_attachedDropTarget, target))
        {
            _attachedDropTarget = null;
        }
    }

    internal HWND GetDropTargetHandleForRegistration() => GetDropTargetHandle();

    internal void VerifyDropTargetAccess()
    {
        if (!ReferenceEquals(Thread.CurrentThread, _thread))
        {
            throw new InvalidOperationException("The calling thread does not own this window.");
        }
    }

    private void DetachAttachedDropTarget(bool throwOnFailure)
        => _attachedDropTarget?.DetachFromWindow(throwOnFailure);

    private void DetachAttachedDragSource(bool throwOnFailure)
        => _attachedDragSource?.DetachFromWindow(throwOnFailure);
}
