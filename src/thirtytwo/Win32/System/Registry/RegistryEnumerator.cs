// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using System.Runtime.ConstrainedExecution;
using Windows.Support;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Windows.Win32.System.Registry;

/// <summary>
///  Enumerates values and subkeys beneath a registry key with consumer-defined filtering and transformation.
/// </summary>
/// <typeparam name="TResult">The result type produced for included registry entries.</typeparam>
public abstract unsafe class RegistryEnumerator<TResult> : CriticalFinalizerObject, IEnumerator<TResult>
{
    private const int InitialBufferCapacity = 256;
    private const int InitialDataCapacity = 4096;
    private const int InitialFrameCapacity = 8;

    private readonly bool _recurseSubKeys;
    private readonly int _maxRecursionDepth;
    private readonly bool _ignoreInaccessible;

    private char[]? _nameBuffer;
    private char[]? _pathBuffer;
    private byte[]? _dataBuffer;
    private RegistryEnumerationFrame[]? _frames;
    private int _frameCount;
    private TResult _current = default!;
    private bool _finished;
    private bool _disposed;

    /// <summary>
    ///  Initializes an enumeration rooted at <paramref name="rootKey"/>.
    /// </summary>
    /// <param name="rootKey">
    ///  Borrowed registry key handle. The caller must keep this handle valid for the lifetime of the enumerator.
    /// </param>
    /// <param name="options">Enumeration options, or <see langword="null"/> for the defaults.</param>
    /// <exception cref="ArgumentException"><paramref name="rootKey"/> is null.</exception>
    protected RegistryEnumerator(HKEY rootKey, RegistryEnumerationOptions? options = null)
    {
        if (rootKey.IsNull)
        {
            throw new ArgumentException("The root registry key handle cannot be null.", nameof(rootKey));
        }

        _recurseSubKeys = options?.RecurseSubKeys ?? false;
        _maxRecursionDepth = options?.MaxRecursionDepth ?? int.MaxValue;
        _ignoreInaccessible = options?.IgnoreInaccessible ?? false;

        try
        {
            _nameBuffer = ArrayPool<char>.Shared.Rent(InitialBufferCapacity);
            _pathBuffer = ArrayPool<char>.Shared.Rent(InitialBufferCapacity);
            _frames = ArrayPool<RegistryEnumerationFrame>.Shared.Rent(InitialFrameCapacity);
            _frames[0] = new RegistryEnumerationFrame(rootKey, ownsKey: false, pathLength: 0, depth: 0);
            _frameCount = 1;
        }
        catch
        {
            ReturnBuffers();
            throw;
        }
    }

    /// <summary>
    ///  Gets the current transformed entry.
    /// </summary>
    public TResult Current => _current;

    /// <inheritdoc/>
    object? IEnumerator.Current => Current;

    /// <summary>
    ///  Determines whether values on the current key should be enumerated.
    /// </summary>
    /// <param name="key">Current key context.</param>
    /// <returns><see langword="true"/> to enumerate the key's values; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldEnumerateValues(ref RegistryKeyContext key) => true;

    /// <summary>
    ///  Determines whether a value should be included in the result stream.
    /// </summary>
    /// <param name="value">Value metadata without data.</param>
    /// <returns><see langword="true"/> to include the value; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldIncludeValue(ref RegistryValueEntry value) => true;

    /// <summary>
    ///  Determines whether value enumeration should continue after processing the current value.
    /// </summary>
    /// <param name="value">The value that was just processed.</param>
    /// <returns><see langword="true"/> to continue with the next value; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldContinueEnumeratingValues(ref RegistryValueEntry value) => true;

    /// <summary>
    ///  Determines whether data should be read for an included value.
    /// </summary>
    /// <param name="value">Included value metadata without data.</param>
    /// <returns><see langword="true"/> to read the value data; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldReadValueData(ref RegistryValueEntry value) => false;

    /// <summary>
    ///  Transforms an included value into a result.
    /// </summary>
    /// <param name="value">Included value, with data when it was requested.</param>
    /// <returns>The transformed result.</returns>
    protected abstract TResult TransformValue(ref RegistryValueEntry value);

    /// <summary>
    ///  Determines whether subkeys on the current key should be enumerated.
    /// </summary>
    /// <param name="key">Current key context.</param>
    /// <returns><see langword="true"/> to enumerate immediate subkeys; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldEnumerateSubKeys(ref RegistryKeyContext key) => true;

    /// <summary>
    ///  Determines whether an immediate subkey should be entered recursively.
    /// </summary>
    /// <param name="key">Discovered subkey.</param>
    /// <returns><see langword="true"/> to enter the subkey; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldRecurseIntoKey(ref RegistryKeyEntry key) => true;

    /// <summary>
    ///  Determines whether a subkey should be included in the result stream.
    /// </summary>
    /// <param name="key">Discovered subkey.</param>
    /// <returns><see langword="true"/> to include the subkey; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldIncludeKey(ref RegistryKeyEntry key) => true;

    /// <summary>
    ///  Transforms an included subkey into a result.
    /// </summary>
    /// <param name="key">Included subkey.</param>
    /// <returns>The transformed result.</returns>
    protected abstract TResult TransformKey(ref RegistryKeyEntry key);

    /// <summary>
    ///  Determines whether enumeration should continue after a native error.
    /// </summary>
    /// <param name="error">Context for the failed operation.</param>
    /// <returns><see langword="true"/> to continue; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ContinueOnError(ref RegistryEnumerationError error) => false;

    /// <summary>
    ///  Called after a key has been fully processed.
    /// </summary>
    /// <param name="key">Finished key context.</param>
    protected virtual void OnKeyFinished(ref RegistryKeyContext key)
    {
    }

    /// <summary>
    ///  Advances to the next included registry entry.
    /// </summary>
    /// <returns><see langword="true"/> when an entry was produced; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext()
    {
        if (_finished || _disposed)
        {
            return false;
        }

        try
        {
            return MoveNextCore();
        }
        catch
        {
            DisposeCore(disposing: true, throwOnFailure: false);
            throw;
        }
    }

    /// <summary>
    ///  Resetting a registry enumeration is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public void Reset() => throw new NotSupportedException();

    /// <summary>
    ///  Releases pooled buffers and closes registry key handles owned by this enumerator.
    /// </summary>
    public void Dispose()
    {
        DisposeCore(disposing: true, throwOnFailure: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Releases resources owned by a derived enumerator.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="true"/> during explicit disposal; <see langword="false"/> during finalization.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
    }

    private bool MoveNextCore()
    {
        while (_frameCount > 0)
        {
            int frameIndex = _frameCount - 1;
            switch (_frames![frameIndex].Phase)
            {
                case RegistryEnumerationPhase.Start:
                    StartValues(frameIndex);
                    break;
                case RegistryEnumerationPhase.Values:
                    if (MoveNextValue(frameIndex))
                    {
                        return true;
                    }

                    break;
                case RegistryEnumerationPhase.StartSubKeys:
                    StartSubKeys(frameIndex);
                    break;
                case RegistryEnumerationPhase.SubKeys:
                    if (MoveNextSubKey(frameIndex))
                    {
                        return true;
                    }

                    break;
                default:
                    FinishKey(frameIndex);
                    break;
            }
        }

        _finished = true;
        ReturnBuffers();
        return false;
    }

    private void StartValues(int frameIndex)
    {
        RegistryKeyContext context = CreateKeyContext(frameIndex);
        _frames![frameIndex].Phase = ShouldEnumerateValues(ref context)
            ? RegistryEnumerationPhase.Values
            : RegistryEnumerationPhase.StartSubKeys;
    }

    private void StartSubKeys(int frameIndex)
    {
        RegistryKeyContext context = CreateKeyContext(frameIndex);
        _frames![frameIndex].Phase = ShouldEnumerateSubKeys(ref context)
            ? RegistryEnumerationPhase.SubKeys
            : RegistryEnumerationPhase.Finished;
    }

    private bool MoveNextValue(int frameIndex)
    {
        HKEY key = _frames![frameIndex].Key;
        uint index = _frames[frameIndex].ValueIndex;
        REG_VALUE_TYPE type;
        int nameLength;
        int dataLength;
        bool performanceData;

        WIN32_ERROR error = ReadValueMetadata(key, index, out nameLength, out type, out dataLength, out performanceData);
        if (error == WIN32_ERROR.ERROR_NO_MORE_ITEMS)
        {
            _frames[frameIndex].Phase = RegistryEnumerationPhase.StartSubKeys;
            return false;
        }

        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            if (!HandleError(
                error,
                RegistryEnumerationOperation.EnumerateValue,
                entryKind: null,
                frameIndex,
                entryName: [],
                _frames[frameIndex].Depth))
            {
                ThrowError(error, frameIndex, []);
            }

            _frames[frameIndex].Phase = RegistryEnumerationPhase.StartSubKeys;
            return false;
        }

        _frames[frameIndex].ValueIndex++;
        ReadOnlySpan<char> name = _nameBuffer.AsSpan(0, nameLength);
        RegistryValueEntry value = new(
            key,
            CurrentPath(frameIndex),
            name,
            _frames[frameIndex].Depth,
            type,
            dataLength,
            hasData: false,
            data: []);

        if (!ShouldIncludeValue(ref value))
        {
            SetValueContinuation(frameIndex, ref value);
            return false;
        }

        if (!ShouldReadValueData(ref value))
        {
            _current = TransformValue(ref value);
            SetValueContinuation(frameIndex, ref value);
            return true;
        }

        error = ReadValueData(key, performanceData, dataLength, out type, out dataLength);
        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            if (!HandleError(
                error,
                RegistryEnumerationOperation.ReadValueData,
                RegistryEntryKind.Value,
                frameIndex,
                name,
                _frames[frameIndex].Depth))
            {
                ThrowError(error, frameIndex, name);
            }

            SetValueContinuation(frameIndex, ref value);
            return false;
        }

        value = new RegistryValueEntry(
            key,
            CurrentPath(frameIndex),
            name,
            _frames[frameIndex].Depth,
            type,
            dataLength,
            hasData: true,
            _dataBuffer.AsSpan(0, dataLength));
        _current = TransformValue(ref value);
        SetValueContinuation(frameIndex, ref value);
        return true;
    }

    private void SetValueContinuation(int frameIndex, ref RegistryValueEntry value)
    {
        if (!ShouldContinueEnumeratingValues(ref value))
        {
            _frames![frameIndex].Phase = RegistryEnumerationPhase.StartSubKeys;
        }
    }

    private WIN32_ERROR ReadValueMetadata(
        HKEY key,
        uint index,
        out int nameLength,
        out REG_VALUE_TYPE type,
        out int dataLength,
        out bool performanceData)
    {
        performanceData = key.IsPerfKey();

        while (true)
        {
            if (performanceData)
            {
                // Performance keys report ERROR_MORE_DATA when data is omitted, so a cleared buffer
                // lets us distinguish a complete null-terminated name from a truncated one.
                _nameBuffer.AsSpan().Clear();
            }

            uint nativeNameLength = checked((uint)_nameBuffer!.Length);
            uint nativeDataLength = 0;
            uint nativeType = 0;

            WIN32_ERROR error;
            fixed (char* namePointer = _nameBuffer)
            {
                error = PInvoke.RegEnumValue(
                    key,
                    index,
                    namePointer,
                    &nativeNameLength,
                    null,
                    &nativeType,
                    null,
                    &nativeDataLength);
            }

            type = (REG_VALUE_TYPE)nativeType;

            if (error == WIN32_ERROR.ERROR_SUCCESS)
            {
                nameLength = checked((int)nativeNameLength);
                dataLength = checked((int)nativeDataLength);
                TerminateName(nameLength);
                return error;
            }

            if (error == WIN32_ERROR.ERROR_MORE_DATA)
            {
                if (performanceData)
                {
                    int terminator = _nameBuffer.AsSpan().IndexOf('\0');
                    if (terminator >= 0)
                    {
                        nameLength = terminator;
                        dataLength = 0;
                        return WIN32_ERROR.ERROR_SUCCESS;
                    }

                    GrowNameBuffer();
                    continue;
                }

                GrowNameBuffer();
                continue;
            }

            nameLength = 0;
            dataLength = 0;
            return error;
        }
    }

    private WIN32_ERROR ReadValueData(
        HKEY key,
        bool performanceData,
        int expectedDataLength,
        out REG_VALUE_TYPE type,
        out int dataLength)
    {
        EnsureDataCapacity(performanceData ? InitialDataCapacity : Math.Max(expectedDataLength, 1));

        while (true)
        {
            uint nativeDataLength = checked((uint)_dataBuffer!.Length);
            REG_VALUE_TYPE nativeType = default;

            WIN32_ERROR error;
            fixed (char* namePointer = _nameBuffer)
            fixed (byte* dataPointer = _dataBuffer)
            {
                error = PInvoke.RegQueryValueEx(
                    key,
                    namePointer,
                    null,
                    &nativeType,
                    dataPointer,
                    &nativeDataLength);
            }

            type = nativeType;

            if (error == WIN32_ERROR.ERROR_SUCCESS)
            {
                dataLength = checked((int)nativeDataLength);
                if (dataLength > _dataBuffer.Length)
                {
                    throw new InvalidOperationException("The registry returned an invalid data length.");
                }

                return error;
            }

            if (error != WIN32_ERROR.ERROR_MORE_DATA)
            {
                dataLength = 0;
                return error;
            }

            if (performanceData)
            {
                EnsureDataCapacity(NextCapacity(_dataBuffer.Length, checked(_dataBuffer.Length + 1)));
            }
            else if (nativeDataLength > _dataBuffer.Length)
            {
                EnsureDataCapacity(checked((int)nativeDataLength));
            }
            else
            {
                EnsureDataCapacity(NextCapacity(_dataBuffer.Length, checked(_dataBuffer.Length + 1)));
            }
        }
    }

    private bool MoveNextSubKey(int frameIndex)
    {
        HKEY parentKey = _frames![frameIndex].Key;
        uint index = _frames[frameIndex].SubKeyIndex;
        uint nativeNameLength = checked((uint)_nameBuffer!.Length);
        FILETIME lastWriteTime = default;

        WIN32_ERROR error;
        fixed (char* namePointer = _nameBuffer)
        {
            error = PInvoke.RegEnumKeyEx(
                parentKey,
                index,
                namePointer,
                &nativeNameLength,
                null,
                default,
                null,
                &lastWriteTime);
        }

        if (error == WIN32_ERROR.ERROR_NO_MORE_ITEMS)
        {
            _frames[frameIndex].Phase = RegistryEnumerationPhase.Finished;
            return false;
        }

        if (error == WIN32_ERROR.ERROR_MORE_DATA)
        {
            GrowNameBuffer();
            return false;
        }

        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            if (!HandleError(
                error,
                RegistryEnumerationOperation.EnumerateSubKey,
                entryKind: null,
                frameIndex,
                entryName: [],
                _frames[frameIndex].Depth))
            {
                ThrowError(error, frameIndex, []);
            }

            _frames[frameIndex].Phase = RegistryEnumerationPhase.Finished;
            return false;
        }

        _frames[frameIndex].SubKeyIndex++;
        int nameLength = checked((int)nativeNameLength);
        TerminateName(nameLength);
        ReadOnlySpan<char> name = _nameBuffer.AsSpan(0, nameLength);
        int childDepth = checked(_frames[frameIndex].Depth + 1);
        int childPathLength = BuildChildPath(_frames[frameIndex].PathLength, name);
        RegistryKeyEntry entry = new(
            parentKey,
            name,
            _pathBuffer.AsSpan(0, childPathLength),
            childDepth,
            lastWriteTime);

        bool recurse = _recurseSubKeys
            && childDepth <= _maxRecursionDepth
            && ShouldRecurseIntoKey(ref entry);
        bool include = ShouldIncludeKey(ref entry);

        if (recurse)
        {
            EnsureFrameCapacity(checked(_frameCount + 1));
            HKEY childKey = default;
            fixed (char* namePointer = _nameBuffer)
            {
                error = PInvoke.RegOpenKeyEx(
                    parentKey,
                    namePointer,
                    0,
                    REG_SAM_FLAGS.KEY_READ,
                    &childKey);
            }

            if (error == WIN32_ERROR.ERROR_SUCCESS)
            {
                _frames![_frameCount++] = new RegistryEnumerationFrame(
                    childKey,
                    ownsKey: true,
                    childPathLength,
                    childDepth);
            }
            else if (!HandleError(
                error,
                RegistryEnumerationOperation.OpenSubKey,
                RegistryEntryKind.Key,
                frameIndex,
                name,
                childDepth))
            {
                ThrowError(error, frameIndex, name);
            }
        }

        if (!include)
        {
            return false;
        }

        _current = TransformKey(ref entry);
        return true;
    }

    private void FinishKey(int frameIndex)
    {
        RegistryKeyContext context = CreateKeyContext(frameIndex);
        OnKeyFinished(ref context);

        RegistryEnumerationFrame frame = _frames![frameIndex];
        _frames[frameIndex] = default;
        _frameCount--;

        if (!frame.OwnsKey)
        {
            return;
        }

        WIN32_ERROR error = PInvoke.RegCloseKey(frame.Key);
        if (error == WIN32_ERROR.ERROR_SUCCESS)
        {
            return;
        }

        ReadOnlySpan<char> path = _pathBuffer.AsSpan(0, frame.PathLength);
        int separator = path.LastIndexOf('\\');
        ReadOnlySpan<char> name = path[(separator + 1)..];
        if (!HandleError(
            error,
            RegistryEnumerationOperation.CloseSubKey,
            RegistryEntryKind.Key,
            frameIndex: -1,
            keyRelativePath: path,
            frame.Key,
            name,
            frame.Depth))
        {
            ThrowError(error, path, name);
        }
    }

    private RegistryKeyContext CreateKeyContext(int frameIndex)
        => new(
            _frames![frameIndex].Key,
            CurrentPath(frameIndex),
            _frames[frameIndex].Depth);

    private ReadOnlySpan<char> CurrentPath(int frameIndex)
        => _pathBuffer.AsSpan(0, _frames![frameIndex].PathLength);

    private int BuildChildPath(int parentPathLength, ReadOnlySpan<char> name)
    {
        int separatorLength = parentPathLength == 0 ? 0 : 1;
        int childPathLength = checked(parentPathLength + separatorLength + name.Length);
        EnsurePathCapacity(childPathLength, parentPathLength);

        Span<char> path = _pathBuffer.AsSpan();
        if (separatorLength != 0)
        {
            path[parentPathLength] = '\\';
        }

        name.CopyTo(path[(parentPathLength + separatorLength)..]);
        return childPathLength;
    }

    private bool HandleError(
        WIN32_ERROR errorCode,
        RegistryEnumerationOperation operation,
        RegistryEntryKind? entryKind,
        int frameIndex,
        ReadOnlySpan<char> entryName,
        int depth)
        => HandleError(
            errorCode,
            operation,
            entryKind,
            frameIndex,
            CurrentPath(frameIndex),
            _frames![frameIndex].Key,
            entryName,
            depth);

    private bool HandleError(
        WIN32_ERROR errorCode,
        RegistryEnumerationOperation operation,
        RegistryEntryKind? entryKind,
        int frameIndex,
        ReadOnlySpan<char> keyRelativePath,
        HKEY key,
        ReadOnlySpan<char> entryName,
        int depth)
    {
        if (_ignoreInaccessible && errorCode == WIN32_ERROR.ERROR_ACCESS_DENIED)
        {
            return true;
        }

        RegistryEnumerationError error = new(
            errorCode,
            operation,
            entryKind,
            key,
            keyRelativePath,
            entryName,
            depth);
        return ContinueOnError(ref error);
    }

    private void ThrowError(WIN32_ERROR error, int frameIndex, ReadOnlySpan<char> entryName)
        => ThrowError(error, CurrentPath(frameIndex), entryName);

    private static void ThrowError(
        WIN32_ERROR error,
        ReadOnlySpan<char> keyRelativePath,
        ReadOnlySpan<char> entryName)
    {
        string path = keyRelativePath.IsEmpty
            ? entryName.ToString()
            : entryName.IsEmpty
                ? keyRelativePath.ToString()
                : string.Concat(keyRelativePath, "\\", entryName);
        error.ThrowThirtyTwoException(path);
    }

    private void TerminateName(int nameLength)
    {
        if ((uint)nameLength >= (uint)_nameBuffer!.Length)
        {
            throw new InvalidOperationException("The registry returned an invalid name length.");
        }

        _nameBuffer[nameLength] = '\0';
    }

    private void GrowNameBuffer()
        => ReplaceNameBuffer(NextCapacity(_nameBuffer!.Length, checked(_nameBuffer.Length + 1)));

    private void ReplaceNameBuffer(int requiredCapacity)
    {
        char[] oldBuffer = _nameBuffer!;
        _nameBuffer = ArrayPool<char>.Shared.Rent(requiredCapacity);
        ArrayPool<char>.Shared.Return(oldBuffer, clearArray: true);
    }

    private void EnsurePathCapacity(int requiredCapacity, int copyLength)
    {
        if (requiredCapacity <= _pathBuffer!.Length)
        {
            return;
        }

        char[] oldBuffer = _pathBuffer;
        char[] newBuffer = ArrayPool<char>.Shared.Rent(NextCapacity(oldBuffer.Length, requiredCapacity));
        oldBuffer.AsSpan(0, copyLength).CopyTo(newBuffer);
        _pathBuffer = newBuffer;
        ArrayPool<char>.Shared.Return(oldBuffer, clearArray: true);
    }

    private void EnsureDataCapacity(int requiredCapacity)
    {
        if (_dataBuffer is not null && requiredCapacity <= _dataBuffer.Length)
        {
            return;
        }

        int capacity = _dataBuffer is null
            ? Math.Max(InitialDataCapacity, requiredCapacity)
            : NextCapacity(_dataBuffer.Length, requiredCapacity);
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(capacity);
        byte[]? oldBuffer = _dataBuffer;
        _dataBuffer = newBuffer;

        if (oldBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(oldBuffer, clearArray: true);
        }
    }

    private void EnsureFrameCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _frames!.Length)
        {
            return;
        }

        RegistryEnumerationFrame[] oldFrames = _frames;
        RegistryEnumerationFrame[] newFrames =
            ArrayPool<RegistryEnumerationFrame>.Shared.Rent(NextCapacity(oldFrames.Length, requiredCapacity));
        oldFrames.AsSpan(0, _frameCount).CopyTo(newFrames);
        _frames = newFrames;
        ArrayPool<RegistryEnumerationFrame>.Shared.Return(oldFrames, clearArray: true);
    }

    private static int NextCapacity(int currentCapacity, int requiredCapacity)
    {
        int doubled = currentCapacity <= int.MaxValue / 2 ? currentCapacity * 2 : int.MaxValue;
        return Math.Max(doubled, requiredCapacity);
    }

    private void DisposeCore(bool disposing, bool throwOnFailure)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _finished = true;
        WIN32_ERROR closeError = WIN32_ERROR.ERROR_SUCCESS;

        if (_frames is not null)
        {
            for (int index = _frameCount - 1; index >= 0; index--)
            {
                RegistryEnumerationFrame frame = _frames[index];
                _frames[index] = default;
                if (frame.OwnsKey)
                {
                    WIN32_ERROR error = PInvoke.RegCloseKey(frame.Key);
                    if (closeError == WIN32_ERROR.ERROR_SUCCESS && error != WIN32_ERROR.ERROR_SUCCESS)
                    {
                        closeError = error;
                    }
                }
            }
        }

        _frameCount = 0;
        ReturnBuffers();

        try
        {
            Dispose(disposing);
        }
        catch when (!throwOnFailure)
        {
        }

        if (throwOnFailure && closeError != WIN32_ERROR.ERROR_SUCCESS)
        {
            closeError.ThrowThirtyTwoException();
        }
    }

    private void ReturnBuffers()
    {
        if (_nameBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_nameBuffer, clearArray: true);
            _nameBuffer = null;
        }

        if (_pathBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_pathBuffer, clearArray: true);
            _pathBuffer = null;
        }

        if (_dataBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_dataBuffer, clearArray: true);
            _dataBuffer = null;
        }

        if (_frames is not null)
        {
            ArrayPool<RegistryEnumerationFrame>.Shared.Return(_frames, clearArray: true);
            _frames = null;
        }
    }

    /// <summary>
    ///  Releases resources if an enumerator that still owns subkey handles is abandoned.
    /// </summary>
    ~RegistryEnumerator() => DisposeCore(disposing: false, throwOnFailure: false);

}