// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public static unsafe class GdiPlus
{
    internal static Session Init() => s_session;
    private static readonly Session s_session = new();

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Initialize() => Init();

    public static nuint Startup(uint version = 2)
    {
        GdiplusStartupInput input = new() { GdiplusVersion = version };
        GdiplusStartupOutput output;
        nuint token;
        ThrowIfFailed(Interop.GdiplusStartup(
            &token,
            &input,
            &output));

        return token;
    }

    public static void Shutdown(nuint token) => Interop.GdiplusShutdown(token);

    public static void ThrowIfFailed(this Status status)
    {
        if (status != Status.Ok)
            throw GetExceptionForStatus(status);
    }

    public static Exception GetExceptionForStatus(Status status)
    {
        switch (status)
        {
            case Status.Win32Error:
                WIN32_ERROR error = Error.GetLastError();
                if (error != WIN32_ERROR.ERROR_SUCCESS)
                    return error.GetException();
                goto default;
            default:
                return new GdiPlusException(status);
        }
    }
}