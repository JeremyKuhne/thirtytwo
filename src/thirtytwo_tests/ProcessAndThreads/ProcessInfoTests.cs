// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Touki.Text;

namespace Windows.ProcessAndThreads;

[TestClass]
public class ProcessInfoTests
{
    [TestMethod]
    public void BasicFunctionality()
    {
        ProcessInfo info = new();
        using ValueStringBuilder builder = new(stackalloc char[256], CultureInfo.CurrentCulture);

        int totalThreads = 0;

        foreach (var process in info)
        {
            builder.AppendLine($"Id: {(long)process.UniqueProcessId} Image Name: {process.ImageName} Threads: {process.NumberOfThreads}");
            totalThreads += (int)process.NumberOfThreads;
        }
    }

    /*
    private void CannotModify()
    {
        ProcessInfo info = new();

        // This doesn't compile as it returns a ref readonly
        info[0].UniqueProcessId = default;
    }
    */
}
