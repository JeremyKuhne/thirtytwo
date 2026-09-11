// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Windows.Win32.System.Registry;

[TestClass]
[DoNotParallelize]
public class RegistryEnumeratorInstallerTests
{
    private const string ComponentsPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Components";

    private static readonly Regex s_hexValueName = new(
        @"\A[0-9A-Fa-f]{32}\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    [TestMethod]
    public void MoveNext_InstallerComponents_MatchesDotNetRegistryCount()
    {
        using global::Microsoft.Win32.RegistryKey managedComponents = OpenManagedComponents();
        int expected = CountWithDotNetRegistry(managedComponents);

        using HKEY components = Registry.OpenKey(
            HKEY.HKEY_LOCAL_MACHINE,
            ComponentsPath,
            REG_SAM_FLAGS.KEY_READ | REG_SAM_FLAGS.KEY_WOW64_64KEY);
        int actual = CountWithRegistryEnumerator(components);

        Console.WriteLine($"Qualifying Installer component keys: {expected:N0}");
        expected.Should().BeGreaterThan(0);
        actual.Should().Be(expected);
    }

    [TestMethod]
    public void MoveNext_HexStringValuePattern_CountsEachImmediateKeyOnce()
    {
        string path = $@"Software\ThirtyTwo.Tests\RegistryEnumeratorInstaller\{Guid.NewGuid():N}";

        try
        {
            using global::Microsoft.Win32.RegistryKey root =
                global::Microsoft.Win32.Registry.CurrentUser.CreateSubKey(path);

            using (global::Microsoft.Win32.RegistryKey key = root.CreateSubKey("Uppercase"))
            {
                key.SetValue("CC64C94C2AA64D3489AF3A7C39B96AAE", "first");
                key.SetValue("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "second");
            }

            using (global::Microsoft.Win32.RegistryKey key = root.CreateSubKey("Lowercase"))
            {
                key.SetValue("cc64c94c2aa64d3489af3a7c39b96aae", "value");
            }

            using (global::Microsoft.Win32.RegistryKey key = root.CreateSubKey("WrongLength"))
            {
                key.SetValue("AAAAAAAAAAAAAAAAAAAAAAAA", "value");
            }

            using (global::Microsoft.Win32.RegistryKey key = root.CreateSubKey("NotHex"))
            {
                key.SetValue("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG", "value");
            }

            using (global::Microsoft.Win32.RegistryKey key = root.CreateSubKey("ExpandString"))
            {
                key.SetValue(
                    "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "%SystemRoot%",
                    global::Microsoft.Win32.RegistryValueKind.ExpandString);
            }

            using HKEY nativeRoot = Registry.OpenKey(HKEY.HKEY_CURRENT_USER, path);

            int expected = CountWithDotNetRegistry(root);
            int actual = CountWithRegistryEnumerator(nativeRoot);

            expected.Should().Be(2);
            actual.Should().Be(expected);
        }
        finally
        {
            global::Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
        }
    }

    private static global::Microsoft.Win32.RegistryKey OpenManagedComponents()
    {
        using global::Microsoft.Win32.RegistryKey localMachine =
            global::Microsoft.Win32.RegistryKey.OpenBaseKey(
                global::Microsoft.Win32.RegistryHive.LocalMachine,
                global::Microsoft.Win32.RegistryView.Registry64);

        return localMachine.OpenSubKey(ComponentsPath)
            ?? throw new InvalidOperationException($@"Registry key not found: HKLM\{ComponentsPath}");
    }

    private static int CountWithDotNetRegistry(global::Microsoft.Win32.RegistryKey components)
    {
        int count = 0;
        foreach (string subKeyName in components.GetSubKeyNames())
        {
            using global::Microsoft.Win32.RegistryKey? component = components.OpenSubKey(subKeyName);
            if (component is null)
            {
                continue;
            }

            foreach (string valueName in component.GetValueNames())
            {
                if (s_hexValueName.IsMatch(valueName)
                    && component.GetValueKind(valueName) == global::Microsoft.Win32.RegistryValueKind.String)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private static int CountWithRegistryEnumerator(HKEY components)
    {
        using InstallerComponentEnumerator enumerator = new(components);
        int count = 0;
        while (enumerator.MoveNext())
        {
            count += enumerator.Current;
        }

        return count;
    }

    private sealed class InstallerComponentEnumerator : RegistryEnumerator<int>
    {
        private static readonly RegistryEnumerationOptions s_options = new()
        {
            RecurseSubKeys = true,
            MaxRecursionDepth = 1,
        };

        private bool _matchedCurrentKey;

        internal InstallerComponentEnumerator(HKEY components) : base(components, s_options)
        {
        }

        protected override bool ShouldEnumerateValues(ref RegistryKeyContext key)
        {
            _matchedCurrentKey = false;
            return key.Depth == 1;
        }

        protected override bool ShouldEnumerateSubKeys(ref RegistryKeyContext key) => key.Depth == 0;

        protected override bool ShouldIncludeValue(ref RegistryValueEntry value)
        {
            if (_matchedCurrentKey
                || value.Type != REG_VALUE_TYPE.REG_SZ
                || !IsHexValueName(value.Name))
            {
                return false;
            }

            _matchedCurrentKey = true;
            return true;
        }

        protected override bool ShouldContinueEnumeratingValues(ref RegistryValueEntry value) => !_matchedCurrentKey;

        protected override bool ShouldIncludeKey(ref RegistryKeyEntry key) => false;

        protected override int TransformKey(ref RegistryKeyEntry key) => 0;

        protected override int TransformValue(ref RegistryValueEntry value) => 1;

        private static bool IsHexValueName(ReadOnlySpan<char> name)
        {
            if (name.Length != 32)
            {
                return false;
            }

            foreach (char character in name)
            {
                if (!char.IsAsciiHexDigit(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}