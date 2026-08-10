// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace TextControlComparison;

internal static class Program
{
    [STAThread]
    private static void Main()
        => Application.Run(static () => new TextControlComparisonWindow());
}