// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Windows.Support;
using Windows.Wdk.System.SystemServices;

namespace Windows.Win32.System.Registry;

public static unsafe partial class Registry
{
    /// <summary>
    ///  Gets the full name of the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This will not work with the "special" root keys such as <see cref="HKEY.HKEY_CLASSES_ROOT"/> as they aren't
    ///   real key handles.
    ///  </para>
    /// </remarks>
    public static string QueryKeyName(HKEY key)
    {
        // The special root keys aren't actually handles so they don't work with this API (returns ERROR_INVALID_HANDLE).

        if (key == HKEY.HKEY_USERS)
        {
            return @"\REGISTRY\USER";
        }
        else if (key == HKEY.HKEY_LOCAL_MACHINE)
        {
            return @"\REGISTRY\MACHINE";
        }
        else if (key == HKEY.HKEY_CURRENT_USER)
        {
            return @"\REGISTRY\USER\.DEFAULT";
        }
        else if (key == HKEY.HKEY_CLASSES_ROOT)
        {
            return @"\REGISTRY\MACHINE\SOFTWARE\CLASSES";
        }

        using BufferScope<char> buffer = new(stackalloc char[256]);
        while (true) fixed (char* b = buffer)
        {
            uint length;
            NTSTATUS status = Wdk.Interop.NtQueryKey(key, KEY_INFORMATION_CLASS.KeyNameInformation, b, (uint)buffer.Length, &length);

            if (status == NTSTATUS.STATUS_BUFFER_TOO_SMALL || status == NTSTATUS.STATUS_BUFFER_OVERFLOW)
            {
                buffer.EnsureCapacity((int)length);
                continue;
            }

            status.ThrowIfFailed();

            KEY_NAME_INFORMATION* nameInfo = (KEY_NAME_INFORMATION*)b;
            return nameInfo->Name.AsSpan((int)nameInfo->NameLength / sizeof(char)).ToString();
        }
    }

    /// <summary>
    ///  Open the specified subkey.
    /// </summary>
    public static HKEY OpenKey(
        HKEY key,
        string? subKeyName,
        REG_SAM_FLAGS rights = REG_SAM_FLAGS.KEY_READ)
    {
        HKEY subKey;
        Interop.RegOpenKeyEx(key, subKeyName, 0, rights, &subKey).ThrowIfFailed();
        return subKey;
    }

    /// <summary>
    ///  Returns true if the given value exists.
    /// </summary>
    public static bool QueryValueExists(HKEY key, string valueName)
    {
        fixed (char* c = valueName)
        {
            WIN32_ERROR result = Interop.RegQueryValueEx(key, c, null, null, null, null);
            return result switch
            {
                WIN32_ERROR.ERROR_SUCCESS => true,
                WIN32_ERROR.ERROR_FILE_NOT_FOUND => false,
                _ => throw Error.GetException(result)
            };
        }
    }

    /// <summary>
    ///  Returns the type of the given value, or REG_NONE if it doesn't exist.
    /// </summary>
    public static REG_VALUE_TYPE QueryValueType(HKEY key, string valueName)
    {
        fixed (char* c = valueName)
        {
            REG_VALUE_TYPE valueType = default;
            WIN32_ERROR result = Interop.RegQueryValueEx(key, c, null, &valueType, null, null);
            return result switch
            {
                WIN32_ERROR.ERROR_SUCCESS => valueType,
                WIN32_ERROR.ERROR_FILE_NOT_FOUND => REG_VALUE_TYPE.REG_NONE,
                _ => throw result.GetException(),
            };
        }
    }

    public static object? QueryValue(HKEY key, string valueName)
    {
        using BufferScope<byte> buffer = new(stackalloc byte[512]);

        fixed (char* n = valueName)
        while (true) fixed (byte* b = buffer)
        {
            REG_VALUE_TYPE valueType = default;

            uint length = (uint)buffer.Length;
            WIN32_ERROR result = Interop.RegQueryValueEx(key, n, null, &valueType, b, &length);

            switch (result)
            {
                case WIN32_ERROR.ERROR_SUCCESS:
                    return ReadValue(buffer[..(int)length], valueType);
                case WIN32_ERROR.ERROR_MORE_DATA:
                    // According to the docs, the byteCapacity given back will not be correct for HKEY_PERFORMANCE_DATA
                    length = length > buffer.Length ? length : checked(length + 256);
                    buffer.EnsureCapacity((int)length);
                    break;
                case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                    return null;
                default:
                    throw result.GetException();
            }
        }
    }

    /// <summary>
    ///  Gets all value names for the given registry key.
    /// </summary>
    public static unsafe IEnumerable<string> GetValueNames(HKEY key)
    {
        uint valueCount;
        uint maxValueNameLength;
        Interop.RegQueryInfoKey(key, default, lpcValues: &valueCount, lpcbMaxValueNameLen: &maxValueNameLength).ThrowIfFailed();

        List<string> names = [];

        // Doesn't include the terminating null.
        maxValueNameLength += 1;

        using BufferScope<char> buffer = maxValueNameLength <= 256
            ? new(stackalloc char[(int)maxValueNameLength]) :
            new((int)maxValueNameLength);

        WIN32_ERROR result = WIN32_ERROR.NO_ERROR;

        while (true) fixed (char* c = buffer)
        {
            uint length = (uint)buffer.Length;
            result = Interop.RegEnumValue(key, (uint)names.Count, c, &length, null, null, null, null);

            switch (result)
            {
                case WIN32_ERROR.ERROR_NO_MORE_ITEMS:
                    return names;
                case WIN32_ERROR.ERROR_SUCCESS:
                    names.Add(buffer.Slice(0, (int)length).ToString());
                    break;
                case WIN32_ERROR.ERROR_MORE_DATA:
                    if (key.IsPerfKey())
                    {
                        // Perf keys always report back ERROR_MORE_DATA,
                        // and also do not report the size of the string.
                        names.Add(buffer.AsSpan().SliceAtNull().ToString());
                    }
                    else
                    {
                        // For some reason the name size isn't reported back,
                        // even though it is known. Why would they not do this?
                        buffer.EnsureCapacity(buffer.Length + 100);
                    }

                    break;
                default:
                    throw result.GetException();
            }
        }
    }

    private static unsafe object ReadValue(ReadOnlySpan<byte> buffer, REG_VALUE_TYPE valueType)
    {
        switch (valueType)
        {
            case REG_VALUE_TYPE.REG_SZ:
            case REG_VALUE_TYPE.REG_EXPAND_SZ:
            case REG_VALUE_TYPE.REG_LINK:
                // Size includes the null
                return MemoryMarshal.Cast<byte, char>(buffer)[..^1].ToString();
            case REG_VALUE_TYPE.REG_MULTI_SZ:
                return MemoryMarshal.Cast<byte, char>(buffer).Split('\0').ToArray();
            case REG_VALUE_TYPE.REG_DWORD_LITTLE_ENDIAN:
                return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            case REG_VALUE_TYPE.REG_DWORD_BIG_ENDIAN:
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            case REG_VALUE_TYPE.REG_QWORD_LITTLE_ENDIAN:
                return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            case REG_VALUE_TYPE.REG_BINARY:
            // The next three aren't defined yet, so we'll just return them as binary blobs
            case REG_VALUE_TYPE.REG_RESOURCE_LIST:                     // CM_RESOURCE_LIST
            case REG_VALUE_TYPE.REG_FULL_RESOURCE_DESCRIPTOR:          // CM_FULL_RESOURCE_DESCRIPTOR
            case REG_VALUE_TYPE.REG_RESOURCE_REQUIREMENTS_LIST:        // CM_RESOURCE_LIST??
                return buffer.ToArray();
            default:
                throw new NotSupportedException($"No support for {valueType} value types.");
        }
    }
}