// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ComponentCategoriesManagerTests
{
    [StaFact]
    public void EnumerateCategories()
    {
        using AgileComPointer<IUnknown> unknown = new(ComHelpers.CreateComClass(CLSID.StdComponentCategoriesManager), takeOwnership: true);
        using var categoriesInfo = unknown.TryGetInterface<ICatInformation>(out HRESULT hr);

        using ComScope<IEnumCATEGORYINFO> enumCategories = new(null);
        hr = categoriesInfo.Value->EnumCategories(Interop.GetUserDefaultLCID(), enumCategories);

        Dictionary<Guid, string> categories = new();
        CATEGORYINFO categoryInfo = default;
        uint fetched = 0;
        while (enumCategories.Value->Next(1, &categoryInfo, &fetched).Succeeded && fetched == 1)
        {
            categories.Add(categoryInfo.catid, categoryInfo.szDescription.ToString());
        }

        categories.Should().Contain(new KeyValuePair<Guid, string>(CATID.Control, "Controls"));
    }

    [StaFact]
    public void EnumerateActiveXClasses()
    {
        using AgileComPointer<IUnknown> unknown = new(ComHelpers.CreateComClass(CLSID.StdComponentCategoriesManager), takeOwnership: true);
        using var categoriesInfo = unknown.TryGetInterface<ICatInformation>(out HRESULT hr);

        using ComScope<IEnumGUID> enumGuids = new(null);
        Guid control = CATID.Control;
        hr = categoriesInfo.Value->EnumClassesOfCategories(1, &control, 0, null, enumGuids);
        hr.Succeeded.Should().BeTrue();

        List<Guid> classGuids = new();
        Guid guid = default;
        uint fetched = 0;
        while (enumGuids.Value->Next(1, &guid, &fetched).Succeeded && fetched == 1)
        {
            classGuids.Add(guid);
        }

        classGuids.Should().Contain(CLSID.WindowsMediaPlayer);
    }
}
