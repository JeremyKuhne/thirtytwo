using System.Runtime.InteropServices;

namespace Windows.Win32;

public static partial class Interop
{
    /// <returns/>
    /// <inheritdoc cref="GetClassLong(HWND, GET_CLASS_LONG_INDEX)"/>
    [DllImport("USER32.dll", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
    public static extern nuint GetClassLongPtr(
        HWND hWnd,
        GET_CLASS_LONG_INDEX nIndex);

    /// <returns/>
    /// <inheritdoc cref="SetWindowLong(HWND, WINDOW_LONG_PTR_INDEX, int)"/>
    [DllImport("USER32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static extern nint SetWindowLongPtr(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex,
        nint dwNewLong);
}
