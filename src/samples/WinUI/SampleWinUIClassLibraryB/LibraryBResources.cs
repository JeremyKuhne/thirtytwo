// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

namespace SampleWinUIClassLibraryB;

public sealed class LibraryBResources : ResourceDictionary
{
    public const string SharedResourceKey = "SampleWinUI.SharedResource";

    public LibraryBResources()
    {
        this["SampleWinUI.LibraryBResource"] = "LibraryB";
        this[SharedResourceKey] = "LibraryB";

        ResourceDictionary defaultTheme = new();
        defaultTheme["SampleWinUI.LibraryBThemeResource"] = "LibraryB.Default";
        ThemeDictionaries["Default"] = defaultTheme;
    }
}