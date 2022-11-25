using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Windows.Support;

public class ThirtyTwoException : Win32Exception
{
    public ThirtyTwoException()
        : base() { }

    public ThirtyTwoException(string? message, HRESULT hresult = default, Exception? innerException = null)
        : base(message, innerException) { HResult = (int)hresult; }

    //public ThirtyTwoIoException(HRESULT result)
    //    : this(Error.HResultToString(result), result) { }

    public ThirtyTwoException(WIN32_ERROR error, string? message = null)
        : base(error.ToHRESULT(), message ?? error.ErrorToString()) { }

    //public ThirtyTwoIoException(NTSTATUS status)
    //    : this(status.ToHResult()) { }
}

public class DriveLockedException : ThirtyTwoIOException
{
    public DriveLockedException(string? message = null)
        : base(HRESULT.FVE_E_LOCKED_VOLUME, message) { }
}

public class FileExistsException : ThirtyTwoIOException
{
    public FileExistsException(WIN32_ERROR error, string? message = null)
        : base(error, message) { }
}

public class DriveNotReadyException : ThirtyTwoIOException
{
    public DriveNotReadyException(string? message = null)
        : base(WIN32_ERROR.ERROR_NOT_READY, message) { }
}

public class ThirtyTwoIOException : IOException
{
    public ThirtyTwoIOException()
        : base() { }

    public ThirtyTwoIOException(HRESULT hr, string? message = null)
        : base(message ?? hr.ToStringWithDescription(), hresult: hr) { }

    public ThirtyTwoIOException(WIN32_ERROR error, string? message = null)
        : base(message ?? error.ErrorToString(), (int)error.ToHRESULT()) { }
}