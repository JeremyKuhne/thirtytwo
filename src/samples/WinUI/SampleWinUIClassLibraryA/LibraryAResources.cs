// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

namespace SampleWinUIClassLibraryA;

public sealed class LibraryAResources : ResourceDictionary
{
    public const string SharedResourceKey = "SampleWinUI.SharedResource";

    public LibraryAResources()
    {
        this["SampleWinUI.LibraryAResource"] = "LibraryA";
        this[SharedResourceKey] = "LibraryA";

        ResourceDictionary defaultTheme = new();
        defaultTheme["SampleWinUI.LibraryAThemeResource"] = "LibraryA.Default";
        ThemeDictionaries["Default"] = defaultTheme;
    }
}