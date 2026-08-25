// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

internal sealed class TextDragSession
{
    private readonly Func<(int Start, int End)> _getSelection;
    private readonly Func<long> _getSourceGeneration;
    private readonly Action<int, int> _setSelection;
    private readonly Action<string> _replaceSelection;
    private readonly int _sourceStart;
    private readonly int _sourceEnd;
    private readonly long _sourceGeneration;
    private int? _sameSourceInsertionStart;
    private int _insertedLength;
    private long? _generationAfterInsertion;

    internal TextDragSession(
        Window source,
        int sourceStart,
        int sourceEnd,
        Func<long> getSourceGeneration,
        Func<(int Start, int End)> getSelection,
        Action<int, int> setSelection,
        Action<string> replaceSelection)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(getSourceGeneration);
        ArgumentNullException.ThrowIfNull(getSelection);
        ArgumentNullException.ThrowIfNull(setSelection);
        ArgumentNullException.ThrowIfNull(replaceSelection);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceStart);
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceEnd, sourceStart);

        Source = source;
        _sourceStart = sourceStart;
        _sourceEnd = sourceEnd;
        _getSourceGeneration = getSourceGeneration;
        _sourceGeneration = getSourceGeneration();
        _getSelection = getSelection;
        _setSelection = setSelection;
        _replaceSelection = replaceSelection;
    }

    internal Window Source { get; }

    internal bool IsSource(Window window) => ReferenceEquals(Source, window);

    internal bool ContainsSourceIndex(Window window, int index)
        => IsSource(window) && index >= _sourceStart && index <= _sourceEnd;

    internal void RecordSameSourceInsertion(Window window, int insertionStart, int insertedLength)
    {
        if (!IsSource(window))
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(insertionStart);
        ArgumentOutOfRangeException.ThrowIfNegative(insertedLength);
        _sameSourceInsertionStart = insertionStart;
        _insertedLength = insertedLength;
        long generationAfterInsertion = _getSourceGeneration();
        _generationAfterInsertion = _sourceGeneration != long.MaxValue
            && generationAfterInsertion == _sourceGeneration + 1
            ? generationAfterInsertion
            : null;
    }

    internal void Complete(DragDropEffects effect)
    {
        if (Source.Handle.IsNull)
        {
            return;
        }

        if (_sameSourceInsertionStart is not null && _generationAfterInsertion is null)
        {
            return;
        }

        long expectedGeneration = _sameSourceInsertionStart is null
            ? _sourceGeneration
            : _generationAfterInsertion!.Value;
        if (_getSourceGeneration() != expectedGeneration)
        {
            return;
        }

        if (effect != DragDropEffects.Move)
        {
            if (_sameSourceInsertionStart is null)
            {
                _setSelection(_sourceStart, _sourceEnd);
            }

            return;
        }

        int sourceLength = checked(_sourceEnd - _sourceStart);
        if (_sameSourceInsertionStart is not { } insertionStart)
        {
            _setSelection(_sourceStart, _sourceEnd);
            _replaceSelection(string.Empty);
            return;
        }

        int deleteStart;
        int destinationStart;
        if (insertionStart < _sourceStart)
        {
            deleteStart = checked(_sourceStart + _insertedLength);
            destinationStart = insertionStart;
        }
        else
        {
            deleteStart = _sourceStart;
            destinationStart = checked(insertionStart - sourceLength);
        }

        _setSelection(deleteStart, checked(deleteStart + sourceLength));
        if (_getSourceGeneration() != expectedGeneration)
        {
            return;
        }

        _replaceSelection(string.Empty);
        _setSelection(destinationStart, checked(destinationStart + _insertedLength));
    }
}
