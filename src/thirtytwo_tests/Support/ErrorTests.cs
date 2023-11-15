// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;

namespace Windows.Support;

[Collection(nameof(ErrorTestCollection))]
public class ErrorTests
{
    [Fact]
    public void Error_FormatMesage()
    {
        // Check an HRESULT with a product string that hopefully isn't localized.
        string message = Error.FormatMessage(HRESULT.FVE_E_LOCKED_VOLUME);
        message.Should().Contain("BitLocker");

        // .NET exception messages aren't localized. (Only .NET Framework)
        message = Error.FormatMessage(HRESULT.COR_E_OBJECTDISPOSED);
        message.Should().Be("Cannot access a disposed object.");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_ACCESS_DENIED);

        string asHResult = Error.FormatMessage(WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT());
        asHResult.Should().Be(message);

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE);
        message.Should().Contain("%1");

        string formatted = message.Replace("%1", "away");

        message = Error.FormatMessage((uint)WIN32_ERROR.ERROR_INVALID_EXE_SIGNATURE, args: "away");
        message.Should().Be(formatted);
    }

    [Theory,
        InlineData(WIN32_ERROR.ERROR_FILE_NOT_FOUND, typeof(FileNotFoundException)),
        InlineData(WIN32_ERROR.ERROR_PATH_NOT_FOUND, typeof(DirectoryNotFoundException)),
        InlineData(WIN32_ERROR.ERROR_ACCESS_DENIED, typeof(UnauthorizedAccessException)),
        InlineData(WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED, typeof(UnauthorizedAccessException)),
        InlineData(WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE, typeof(PathTooLongException)),
        InlineData(WIN32_ERROR.ERROR_INVALID_DRIVE, typeof(DriveNotFoundException)),
        InlineData(WIN32_ERROR.ERROR_OPERATION_ABORTED, typeof(OperationCanceledException)),
        InlineData(WIN32_ERROR.ERROR_NOT_READY, typeof(DriveNotReadyException)),
        InlineData(WIN32_ERROR.ERROR_ALREADY_EXISTS, typeof(FileExistsException)),
        InlineData(WIN32_ERROR.ERROR_SHARING_VIOLATION, typeof(ThirtyTwoException)),
        InlineData(WIN32_ERROR.ERROR_FILE_EXISTS, typeof(FileExistsException))
        ]
    public void ErrorsMapToExceptions(WIN32_ERROR error, Type exceptionType)
    {
        error.GetException().Should().BeOfType(exceptionType);
    }

    [Theory,
        InlineData(0, @"ERROR_SUCCESS (0): The operation completed successfully. "),
        InlineData(2, @"ERROR_FILE_NOT_FOUND (2): The system cannot find the file specified. "),
        InlineData(3, @"ERROR_PATH_NOT_FOUND (3): The system cannot find the path specified. ")
        ]
    public void WindowsErrorTextIsAsExpected(uint error, string expected)
    {
        Error.ErrorToString((WIN32_ERROR)error).Should().Be(expected);
    }
}