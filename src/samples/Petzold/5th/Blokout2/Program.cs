// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace Blokout2;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 7-11, Pages 314-317.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Blockout2("Mouse Button & Capture Demo"));
}
