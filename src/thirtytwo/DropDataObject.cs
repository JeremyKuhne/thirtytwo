// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

/// <summary>Provides synchronous access to data offered during an OLE drag-and-drop callback.</summary>
/// <remarks>Instances are valid only while the associated drag event handler is running.</remarks>
public sealed unsafe class DropDataObject
{
    private const uint AllFiles = uint.MaxValue;

    /// <summary>The default maximum number of Unicode characters copied from a dropped data object.</summary>
    public const int DefaultMaximumTextLength = 16 * 1024 * 1024;

    /// <summary>The default maximum number of file paths copied from a dropped data object.</summary>
    public const int DefaultMaximumFileCount = 4096;

    /// <summary>The default maximum length of one dropped file path.</summary>
    public const int DefaultMaximumFilePathLength = 32_767;

    /// <summary>The default maximum combined length of all dropped file paths.</summary>
    public const int DefaultMaximumTotalFilePathLength = 1024 * 1024;

    private nint _dataObject;

    internal DropDataObject(IDataObject* dataObject)
        => _dataObject = (nint)dataObject;

    /// <summary>Gets whether Unicode text is available as global memory.</summary>
    public bool ContainsText => ContainsFormat((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);

    /// <summary>Gets whether a file-path list is available as global memory.</summary>
    public bool ContainsFilePaths => ContainsFormat((uint)CLIPBOARD_FORMAT.CF_HDROP);

    /// <summary>Gets whether the specified clipboard format is available as global memory.</summary>
    public bool ContainsFormat(uint format)
    {
        FORMATETC formatEtc = CreateFormat(format);
        HRESULT result = GetDataObject()->QueryGetData(&formatEtc);
        if (result.Succeeded)
        {
            return true;
        }

        if (result == PInvoke.DV_E_FORMATETC
            || result == PInvoke.DV_E_TYMED
            || result == PInvoke.DV_E_DVASPECT
            || result == PInvoke.DV_E_LINDEX)
        {
            return false;
        }

        result.ThrowOnFailure();
        return false;
    }

    /// <summary>Gets Unicode text, or <see langword="null"/> when that format is unavailable.</summary>
    /// <param name="maximumCharacterCount">The maximum accepted character count.</param>
    public string? GetText(int maximumCharacterCount = DefaultMaximumTextLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumCharacterCount);
        if (!ContainsText)
        {
            return null;
        }

        FORMATETC format = CreateFormat((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);
        STGMEDIUM medium = GetMedium(format);
        try
        {
            HGLOBAL global = GetGlobalHandle(medium);
            nuint byteLength = PInvoke.GlobalSize(global);
            if (byteLength == 0)
            {
                return string.Empty;
            }

            int characterLength = GetUnicodeCharacterLength(byteLength, maximumCharacterCount);

            void* memory = PInvoke.GlobalLock(global);
            if (memory is null)
            {
                Error.GetLastError().ThrowThirtyTwoException();
            }

            try
            {
                ReadOnlySpan<char> characters = new(memory, characterLength);
                ReadOnlySpan<char> text = Touki.SpanExtensions.SliceAtNull(characters);
                if (text.Length > maximumCharacterCount)
                {
                    throw new InvalidDataException(
                        $"Dropped Unicode text exceeds the {maximumCharacterCount:N0} character limit.");
                }

                return text.ToString();
            }
            finally
            {
                if (!PInvoke.GlobalUnlock(global))
                {
                    WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
                }
            }
        }
        finally
        {
            PInvoke.ReleaseStgMedium(&medium);
        }
    }

    /// <summary>Gets dropped file paths, or an empty list when that format is unavailable.</summary>
    /// <param name="maximumFileCount">The maximum accepted number of files.</param>
    /// <param name="maximumPathLength">The maximum accepted length of one path.</param>
    public IReadOnlyList<string> GetFilePaths(
        int maximumFileCount = DefaultMaximumFileCount,
        int maximumPathLength = DefaultMaximumFilePathLength,
        int maximumTotalPathLength = DefaultMaximumTotalFilePathLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumFileCount);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumPathLength);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumTotalPathLength);
        if (!ContainsFilePaths)
        {
            return [];
        }

        FORMATETC format = CreateFormat((uint)CLIPBOARD_FORMAT.CF_HDROP);
        STGMEDIUM medium = GetMedium(format);
        try
        {
            HDROP drop = (HDROP)(nint)GetGlobalHandle(medium);
            uint count = PInvoke.DragQueryFile(drop, AllFiles, null, 0);
            if (count > (uint)maximumFileCount)
            {
                throw new InvalidDataException(
                    $"Dropped file count exceeds the {maximumFileCount:N0} file limit.");
            }

            List<string> paths = new(checked((int)count));
            using BufferScope<char> buffer = new(stackalloc char[256]);
            int totalPathLength = 0;
            for (uint index = 0; index < count; index++)
            {
                uint length = PInvoke.DragQueryFile(drop, index, null, 0);
                if (length > (uint)maximumPathLength)
                {
                    throw new InvalidDataException(
                        $"Dropped file path exceeds the {maximumPathLength:N0} character limit.");
                }

                if (length >= int.MaxValue)
                {
                    throw new InvalidDataException("A dropped file path is too long to copy.");
                }

                if (length > (uint)(maximumTotalPathLength - totalPathLength))
                {
                    throw new InvalidDataException(
                        $"Combined dropped file paths exceed the {maximumTotalPathLength:N0} character limit.");
                }

                totalPathLength += (int)length;

                buffer.EnsureCapacity((int)length + 1);
                fixed (char* path = buffer)
                {
                    uint copied = PInvoke.DragQueryFile(drop, index, path, (uint)buffer.Length);
                    if (copied != length)
                    {
                        throw new InvalidDataException("A dropped file path could not be read completely.");
                    }

                    paths.Add(new string(path, 0, checked((int)copied)));
                }
            }

            return paths;
        }
        finally
        {
            PInvoke.ReleaseStgMedium(&medium);
        }
    }

    internal void Invalidate() => _dataObject = 0;

    private static int GetUnicodeCharacterLength(nuint byteLength, int maximumCharacterCount)
    {
        nuint maximumBufferCharacterCount = maximumCharacterCount == int.MaxValue
            ? int.MaxValue
            : (nuint)maximumCharacterCount + 1;
        nuint maximumByteLength = checked(maximumBufferCharacterCount * sizeof(char));
        if (byteLength > maximumByteLength)
        {
            throw new InvalidDataException(
                $"Dropped Unicode text exceeds the {maximumCharacterCount:N0} character limit.");
        }

        if ((byteLength & 1) != 0)
        {
            throw new InvalidDataException("Dropped Unicode text has an odd byte length.");
        }

        return (int)(byteLength / sizeof(char));
    }

    private static FORMATETC CreateFormat(uint format)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format, ushort.MaxValue);
        return new()
        {
            cfFormat = (ushort)format,
            dwAspect = (uint)DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            tymed = (uint)TYMED.TYMED_HGLOBAL
        };
    }

    private STGMEDIUM GetMedium(FORMATETC format)
    {
        STGMEDIUM medium = default;
        GetDataObject()->GetData(&format, &medium).ThrowOnFailure();
        return medium;
    }

    private static HGLOBAL GetGlobalHandle(STGMEDIUM medium)
    {
        if (medium.tymed != TYMED.TYMED_HGLOBAL || medium.u.hGlobal.IsNull)
        {
            throw new InvalidDataException("The dropped data was not returned as global memory.");
        }

        return medium.u.hGlobal;
    }

    private IDataObject* GetDataObject()
    {
        ObjectDisposedException.ThrowIf(_dataObject == 0, this);
        return (IDataObject*)_dataObject;
    }
}
