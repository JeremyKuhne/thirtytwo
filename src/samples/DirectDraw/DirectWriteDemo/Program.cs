// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace DirectWriteDemo;

internal partial class Program
{
    [STAThread]
    private static void Main() => Application.Run(new DirectWriteDemo());
}
