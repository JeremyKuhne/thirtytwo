// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Microsoft.Win32;
using Windows.Win32.System.Registry;
using NativeRegistry = Windows.Win32.System.Registry.Registry;

namespace thirtytwo.perf;

/// <summary>
///  Compares Installer component-key counting through the managed registry API and <see cref="RegistryEnumerator{TResult}"/>.
/// </summary>
[MemoryDiagnoser]
public class RegistryEnumeratorInstallerBenchmarks
{
    private const string ComponentsPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Components";

    private RegistryKey? _managedComponents;
    private HKEY _nativeComponents;
    private int _expectedCount;

    /// <summary>
    ///  Opens both 64-bit registry views and verifies that the benchmark implementations agree.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _managedComponents = OpenManagedComponents();

        try
        {
            _nativeComponents = NativeRegistry.OpenKey(
                HKEY.HKEY_LOCAL_MACHINE,
                ComponentsPath,
                REG_SAM_FLAGS.KEY_READ | REG_SAM_FLAGS.KEY_WOW64_64KEY);

            _expectedCount = CountWithDotNetRegistry(_managedComponents);
            int actual = InstallerComponentRegistryEnumerator.Count(_nativeComponents);

            if (_expectedCount <= 0)
            {
                throw new InvalidOperationException("The Installer Components scenario produced no qualifying keys.");
            }

            if (actual != _expectedCount)
            {
                throw new InvalidOperationException(
                    $"Registry counts differ. Microsoft.Win32: {_expectedCount}; RegistryEnumerator: {actual}.");
            }
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    /// <summary>
    ///  Closes the registry handles opened by <see cref="Setup"/>.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        if (!_nativeComponents.IsNull)
        {
            _nativeComponents.Dispose();
            _nativeComponents = default;
        }

        _managedComponents?.Dispose();
        _managedComponents = null;
    }

    /// <summary>
    ///  Counts qualifying keys using only <see cref="RegistryKey"/> APIs.
    /// </summary>
    /// <returns>The number of qualifying immediate component keys.</returns>
    [Benchmark(Baseline = true)]
    public int DotNetRegistry()
        => ValidateCount(CountWithDotNetRegistry(_managedComponents!));

    /// <summary>
    ///  Counts qualifying keys using <see cref="RegistryEnumerator{TResult}"/>.
    /// </summary>
    /// <returns>The number of qualifying immediate component keys.</returns>
    [Benchmark]
    public int RegistryEnumerator()
        => ValidateCount(InstallerComponentRegistryEnumerator.Count(_nativeComponents));

    private static RegistryKey OpenManagedComponents()
    {
        using RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        return localMachine.OpenSubKey(ComponentsPath)
            ?? throw new InvalidOperationException($@"Registry key not found: HKLM\{ComponentsPath}");
    }

    private static int CountWithDotNetRegistry(RegistryKey components)
    {
        int count = 0;
        foreach (string subKeyName in components.GetSubKeyNames())
        {
            using RegistryKey? component = components.OpenSubKey(subKeyName);
            if (component is null)
            {
                continue;
            }

            foreach (string valueName in component.GetValueNames())
            {
                if (InstallerComponentRegistryEnumerator.IsHexValueName(valueName)
                    && component.GetValueKind(valueName) == RegistryValueKind.String)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private int ValidateCount(int count)
    {
        if (count != _expectedCount)
        {
            throw new InvalidOperationException(
                $"The registry changed during measurement. Expected {_expectedCount} qualifying keys; found {count}.");
        }

        return count;
    }
}