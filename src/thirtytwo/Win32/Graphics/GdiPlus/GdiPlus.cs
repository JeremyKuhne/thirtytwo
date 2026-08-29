// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  <para>
///   Provides process-level GDI+ initialization and status handling helpers.
///  </para>
/// </summary>
public static unsafe class GdiPlus
{
    /// <summary>
    ///  <para>
    ///   Gets the lazily initialized process-level GDI+ session.
    ///  </para>
    /// </summary>
    /// <returns>
    ///  <para>
    ///   The process-level GDI+ session.
    ///  </para>
    /// </returns>
    internal static Session Init() => s_session;
    private static readonly Session s_session = new();

    /// <summary>
    ///  <para>
    ///   Ensures that a default process-level GDI+ session has been created.
    ///  </para>
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Initialize() => Init();

    /// <summary>
    ///  <para>
    ///   Starts a GDI+ session and returns its token.
    ///  </para>
    /// </summary>
    /// <param name="version">
    ///  <para>
    ///   The GDI+ startup version to request.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   A token that must be passed to <see cref="Shutdown(nuint)"/>.
    ///  </para>
    /// </returns>
    /// <exception cref="Exception">
    ///  <para>
    ///   The underlying startup call failed.
    ///  </para>
    /// </exception>
    public static nuint Startup(uint version = 2)
    {
        GdiplusStartupInput input = new() { GdiplusVersion = version };
        GdiplusStartupOutput output;
        nuint token;
        ThrowIfFailed(PInvoke.GdiplusStartup(
            &token,
            &input,
            &output));

        return token;
    }

    /// <summary>
    ///  <para>
    ///   Shuts down a GDI+ session created by <see cref="Startup(uint)"/>.
    ///  </para>
    /// </summary>
    /// <param name="token">
    ///  <para>
    ///   The session token returned by <see cref="Startup(uint)"/>.
    ///  </para>
    /// </param>
    public static void Shutdown(nuint token) => PInvoke.GdiplusShutdown(token);

    /// <summary>
    ///  <para>
    ///   Throws an exception if a GDI+ status value is not <see cref="Status.Ok"/>.
    ///  </para>
    /// </summary>
    /// <param name="status">
    ///  <para>
    ///   The status value to evaluate.
    ///  </para>
    /// </param>
    /// <exception cref="Exception">
    ///  <para>
    ///   Thrown when <paramref name="status"/> indicates failure.
    ///  </para>
    /// </exception>
    public static void ThrowIfFailed(this Status status)
    {
        if (status != Status.Ok)
            throw GetExceptionForStatus(status);
    }

    /// <summary>
    ///  <para>
    ///   Maps a failed GDI+ status value to an exception instance.
    ///  </para>
    /// </summary>
    /// <param name="status">
    ///  <para>
    ///   The failed status value.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   A mapped exception for <paramref name="status"/>.
    ///  </para>
    /// </returns>
    /// <remarks>
    ///  <para>
    ///   <see cref="Status.Win32Error"/> maps to the current <c>GetLastError</c> value when available; other
    ///   statuses map to <see cref="GdiPlusException"/>.
    ///  </para>
    /// </remarks>
    public static Exception GetExceptionForStatus(Status status)
    {
        switch (status)
        {
            case Status.Win32Error:
                WIN32_ERROR error = Error.GetLastError();
                if (error != WIN32_ERROR.ERROR_SUCCESS)
                    return error.GetThirtyTwoException();
                goto default;
            default:
                return new GdiPlusException(status);
        }
    }
}