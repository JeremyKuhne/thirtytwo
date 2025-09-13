// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

public class RegistryTests
{
    [Fact]
    public void Registry_OpenKey_UserKey()
    {
        using HKEY key = Registry.OpenKey(HKEY.HKEY_CURRENT_USER, null);
        key.IsNull.Should().BeFalse();
    }

    [Fact]
    public void Registry_QueryKeyName_UserKey()
    {
        using HKEY key = Registry.OpenKey(HKEY.HKEY_CURRENT_USER, null);
        Registry.QueryKeyName(key).Should().Be(@"\REGISTRY\USER\.DEFAULT");
    }

    [Fact]
    public void Registry_QueryKeyName_ClassesRoot()
    {
        using HKEY key = Registry.OpenKey(HKEY.HKEY_CLASSES_ROOT, null);
        Registry.QueryKeyName(key).Should().Be(@"\REGISTRY\MACHINE\SOFTWARE\CLASSES");
    }

    [Fact]
    public void Registry_OpenKey_UserSubKey()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");

        key.IsNull.Should().BeFalse();
    }

    [Fact]
    public void Registry_QueryKeyName_UserSubKey()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");

        // \REGISTRY\USER\S-1-5-21-3004159937-2065658190-3839796541-1001\Software\Microsoft\Windows NT\CurrentVersion\Winlogon
        Registry.QueryKeyName(key).Should().StartWith(@"\REGISTRY\USER\S-1").And.EndWithEquivalentOf(@"\Software\Microsoft\Windows NT\CurrentVersion\Winlogon");
    }


    [Fact]
    public void HKEY_IsLocalKey()
    {
        using HKEY key = Registry.OpenKey(HKEY.HKEY_CURRENT_USER, null);
        key.IsLocalKey.Should().BeFalse();
    }

    [Fact]
    public void HKEY_IsSpecialKey()
    {
        using HKEY key = Registry.OpenKey(HKEY.HKEY_CLASSES_ROOT, null);
        key.IsNull.Should().BeFalse();
        key.IsSpecialKey.Should().BeFalse();
    }

    [Fact]
    public void Registry_QueryValueExists()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        Registry.QueryValueExists(key, "BuildNumber").Should().BeTrue();
    }

    [Fact]
    public void Registry_QueryValueExists_False()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        Registry.QueryValueExists(key, "Fizzlewig").Should().BeFalse();
    }

    [Fact]
    public void Registry_QueryValueType()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        Registry.QueryValueType(key, "BuildNumber").Should().Be(REG_VALUE_TYPE.REG_DWORD);
    }

    [Fact]
    public void Registry_GetValueNames()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        var names = Registry.GetValueNames(key);
        names.Should().Contain("BuildNumber");
    }

    [Fact]
    public void Registry_GetValueNames_PerformanceData()
    {
        var names = Registry.GetValueNames(HKEY.HKEY_PERFORMANCE_DATA);
        names.Should().ContainInOrder("Global", "Costly");
    }

    [Fact]
    public void Registry_GetValueNames_PerformanceText()
    {
        var names = Registry.GetValueNames(HKEY.HKEY_PERFORMANCE_TEXT);
        names.Should().ContainInOrder("Counter", "Help");
    }

    [Fact]
    public void Registry_GetValueNames_PerformanceNlsText()
    {
        var names = Registry.GetValueNames(HKEY.HKEY_PERFORMANCE_NLSTEXT);
        names.Should().ContainInOrder("Counter", "Help");
    }

    [Fact]
    public void Registry_QueryValue_Uint()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        var buildNumber = Registry.QueryValue(key, "BuildNumber");
        buildNumber.Should().BeOfType<uint>();
    }

    [Fact]
    public void Registry_QueryValue_String()
    {
        using HKEY key = Registry.OpenKey(
            HKEY.HKEY_LOCAL_MACHINE,
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        object? productName = Registry.QueryValue(key, "ProductName");
        productName.Should().BeOfType<string>().Which.Should().StartWith("Windows");
    }
}
