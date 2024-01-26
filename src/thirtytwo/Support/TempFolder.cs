// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using IO = System.IO;

namespace Windows.Support;

public sealed class TempFolder : IDisposable
{
    public string Path { get; } = IO.Path.Join(IO.Path.GetTempPath(), IO.Path.GetRandomFileName());

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch
        {
        }
    }
}