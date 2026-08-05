// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace OwnDraw;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 9-3, Pages 375-380.
/// </summary>
internal static partial class Program
{
    [STAThread]
    private static void Main() => Application.Run(new OwnerDraw("Owner-Draw Button Demo"));
}
