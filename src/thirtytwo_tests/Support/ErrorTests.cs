// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;

namespace Windows.Support;

[DoNotParallelize]
[TestClass]
public class ErrorTests
{
    [TestMethod]
    [Retry(5)]
    public void Error_FormatMessage_RuntimeError()
    {
        // The Marshal.GetExceptionForHR method is not thread safe for CLR (COR_E*) HRESULTs.
        // We don't control all threads in the process, so we have to retry a few times.

        // .NET exception messages aren't localized. (Only .NET Framework)
        string message = Error.FormatMessage(HRESULT.COR_E_OBJECTDISPOSED);
        message.Should().Be("Cannot access a disposed object.");
    }

    [TestMethod]
    public void Error_FormatMesage()
    {
        // Check an HRESULT with a product string that hopefully isn't localized.
        string message = Error.FormatMessage(HRESULT.FVE_E_LOCKED_VOLUME);
        message.Should().Contain("BitLocker");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_ACCESS_DENIED);

        string asHResult = Error.FormatMessage(WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT());
        asHResult.Should().Be(message);

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE);
        message.Should().Contain("%1");

        string formatted = message.Replace("%1", "away");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE, args: "away");
        message.Should().Be(formatted);
    }

    [TestMethod,
        DataRow(WIN32_ERROR.ERROR_FILE_NOT_FOUND, typeof(FileNotFoundException)),
        DataRow(WIN32_ERROR.ERROR_PATH_NOT_FOUND, typeof(DirectoryNotFoundException)),
        DataRow(WIN32_ERROR.ERROR_ACCESS_DENIED, typeof(UnauthorizedAccessException)),
        DataRow(WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED, typeof(UnauthorizedAccessException)),
        DataRow(WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE, typeof(PathTooLongException)),
        DataRow(WIN32_ERROR.ERROR_INVALID_DRIVE, typeof(DriveNotFoundException)),
        DataRow(WIN32_ERROR.ERROR_OPERATION_ABORTED, typeof(OperationCanceledException)),
        DataRow(WIN32_ERROR.ERROR_NOT_READY, typeof(DriveNotReadyException)),
        DataRow(WIN32_ERROR.ERROR_ALREADY_EXISTS, typeof(FileExistsException)),
        DataRow(WIN32_ERROR.ERROR_SHARING_VIOLATION, typeof(ThirtyTwoException)),
        DataRow(WIN32_ERROR.ERROR_FILE_EXISTS, typeof(FileExistsException))
        ]
    public void ErrorsMapToExceptions(WIN32_ERROR error, Type exceptionType)
    {
        error.GetException().Should().BeOfType(exceptionType);
    }

    [TestMethod,
        DataRow(0u, @"ERROR_SUCCESS (0): The operation completed successfully. "),
        DataRow(2u, @"ERROR_FILE_NOT_FOUND (2): The system cannot find the file specified. "),
        DataRow(3u, @"ERROR_PATH_NOT_FOUND (3): The system cannot find the path specified. ")
        ]
    public void WindowsErrorTextIsAsExpected(uint error, string expected)
    {
        Error.ErrorToString((WIN32_ERROR)error).Should().Be(expected);
    }
}