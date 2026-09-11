// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Registry;

[TestClass]
public class RegistryEnumeratorTests
{
    [TestMethod]
    public void MoveNext_Nonrecursive_EnumeratesRootValuesBeforeImmediateSubkeys()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("RootValue", "root");
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("ChildValue", "child");

            using RecordingRegistryEnumerator enumerator = new(root);
            enumerator.ReadValueNames.Add("RootValue");

            Drain(enumerator).Should().Equal(
                "V::RootValue:root",
                "K:Child");
            enumerator.FinishedPaths.Should().Equal(string.Empty);
        });
    }

    [TestMethod]
    public void MoveNext_Recursive_EnumeratesValuesFirstDepthFirstAndFinishesDeepestFirst()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("RootValue", "root");
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("ChildValue", "child");
            using global::Microsoft.Win32.RegistryKey grandchild = child.CreateSubKey("Grandchild");
            grandchild.SetValue("GrandchildValue", "grandchild");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options);
            enumerator.ReadAllValues = true;

            Drain(enumerator).Should().Equal(
                "V::RootValue:root",
                "K:Child",
                "V:Child:ChildValue:child",
                @"K:Child\Grandchild",
                @"V:Child\Grandchild:GrandchildValue:grandchild");
            enumerator.FinishedPaths.Should().Equal(
                @"Child\Grandchild",
                "Child",
                string.Empty);
            enumerator.KeyDepths["Child"].Should().Be(1);
            enumerator.KeyDepths[@"Child\Grandchild"].Should().Be(2);
            enumerator.ValueDepths[":RootValue"].Should().Be(0);
            enumerator.ValueDepths["Child:ChildValue"].Should().Be(1);
            enumerator.ValueDepths[@"Child\Grandchild:GrandchildValue"].Should().Be(2);
            enumerator.KeysWithWriteTimes.Should().Contain("Child").And.Contain(@"Child\Grandchild");
            enumerator.AllCallbackHandlesValid.Should().BeTrue();
        });
    }

    [TestMethod]
    public void MoveNext_RecursiveSiblings_RestoresParentPath()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey first = managedRoot.CreateSubKey("First");
            first.SetValue("Value", "first");
            using global::Microsoft.Win32.RegistryKey second = managedRoot.CreateSubKey("Second");
            second.SetValue("Value", "second");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options) { ReadAllValues = true };

            string[] results = Drain(enumerator);

            results.Should().BeEquivalentTo(
                "K:First",
                "V:First:Value:first",
                "K:Second",
                "V:Second:Value:second");

            foreach (string childName in new[] { "First", "Second" })
            {
                int keyIndex = Array.IndexOf(results, $"K:{childName}");
                results[keyIndex + 1].Should().StartWith($"V:{childName}:");
            }
        });
    }

    [TestMethod]
    public void MoveNext_ExcludedKey_CanStillBeRecursed()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey hidden = managedRoot.CreateSubKey("Hidden");
            hidden.SetValue("Descendant", "value");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options);
            enumerator.ExcludedKeyNames.Add("Hidden");

            Drain(enumerator).Should().Equal("V:Hidden:Descendant:<unread>");
            enumerator.FinishedPaths.Should().Equal("Hidden", string.Empty);
        });
    }

    [TestMethod]
    public void MoveNext_IncludedKey_CanRejectRecursion()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("Descendant", "value");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options);
            enumerator.NonrecursiveKeyNames.Add("Child");

            Drain(enumerator).Should().Equal("K:Child");
            enumerator.FinishedPaths.Should().Equal(string.Empty);
        });
    }

    [TestMethod]
    public void MoveNext_MaxRecursionDepth_DoesNotEnterKeysPastLimit()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("ChildValue", "child");
            using global::Microsoft.Win32.RegistryKey grandchild = child.CreateSubKey("Grandchild");
            grandchild.SetValue("GrandchildValue", "grandchild");

            RegistryEnumerationOptions options = new()
            {
                RecurseSubKeys = true,
                MaxRecursionDepth = 1,
            };

            using RecordingRegistryEnumerator enumerator = new(root, options);

            Drain(enumerator).Should().Equal(
                "K:Child",
                "V:Child:ChildValue:<unread>",
                @"K:Child\Grandchild");
            enumerator.FinishedPaths.Should().Equal("Child", string.Empty);
        });
    }

    [TestMethod]
    public void MoveNext_EnumerationGates_SkipSelectedKeyContents()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("RootValue", "root");
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");

            using RecordingRegistryEnumerator noValues = new(root);
            noValues.SkipValuePaths.Add(string.Empty);
            Drain(noValues).Should().Equal("K:Child");

            using RecordingRegistryEnumerator noSubKeys = new(root);
            noSubKeys.SkipSubKeyPaths.Add(string.Empty);
            Drain(noSubKeys).Should().Equal("V::RootValue:<unread>");
        });
    }

    [TestMethod]
    public void MoveNext_MatchingValues_ReadDataOnlyWhenRequested()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("Read", "payload");
            managedRoot.SetValue("MetadataOnly", "ignored");

            using RecordingRegistryEnumerator enumerator = new(root);
            enumerator.ReadValueNames.Add("Read");
            _ = Drain(enumerator);

            enumerator.ValueStates["Read"].Should().Be((true, "payload"));
            enumerator.ValueStates["MetadataOnly"].Should().Be((false, "<unread>"));
            enumerator.DeclaredDataLengths["MetadataOnly"].Should().BeGreaterThan(0);
            enumerator.ValueDataLengths["MetadataOnly"].Should().Be(0);
        });
    }

    [TestMethod]
    public void MoveNext_RejectedValue_IsNotReadOrTransformed()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("Rejected", "payload");

            using RecordingRegistryEnumerator enumerator = new(root);
            enumerator.ExcludedValueNames.Add("Rejected");
            enumerator.ReadValueNames.Add("Rejected");

            Drain(enumerator).Should().BeEmpty();

            enumerator.ReadDecisionNames.Should().NotContain("Rejected");
            enumerator.ValueStates.Should().NotContainKey("Rejected");
        });
    }

    [TestMethod]
    public void MoveNext_ValueContinuationRejected_StopsCurrentKeyAfterOneValue()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("First", "first");
            managedRoot.SetValue("Second", "second");

            using RecordingRegistryEnumerator enumerator = new(root) { StopAfterValue = true };

            Drain(enumerator).Should().ContainSingle();
            enumerator.ContinueValueNames.Should().ContainSingle();
            enumerator.ValueStates.Should().ContainSingle();
        });
    }

    [TestMethod]
    public void MoveNext_RequestedLargeValue_GrowsAndReturnsCompleteData()
    {
        WithTestKey((managedRoot, root) =>
        {
            byte[] data = new byte[16 * 1024];
            Random.Shared.NextBytes(data);
            managedRoot.SetValue("Large", data, global::Microsoft.Win32.RegistryValueKind.Binary);

            using RecordingRegistryEnumerator enumerator = new(root);
            enumerator.ReadValueNames.Add("Large");
            _ = Drain(enumerator);

            enumerator.ValueDataLengths["Large"].Should().Be(data.Length);
            enumerator.ValueChecksums["Large"].Should().Be(Checksum(data));
        });
    }

    [TestMethod]
    public void MoveNext_ValueGrowsBetweenMetadataAndDataRead_GrowsAndReturnsCurrentData()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("Growing", Array.Empty<byte>(), global::Microsoft.Win32.RegistryValueKind.Binary);
            byte[] currentData = new byte[16 * 1024];
            Random.Shared.NextBytes(currentData);

            using RecordingRegistryEnumerator enumerator = new(root)
            {
                BeforeRead = name => managedRoot.SetValue(
                    name,
                    currentData,
                    global::Microsoft.Win32.RegistryValueKind.Binary),
            };
            enumerator.ReadValueNames.Add("Growing");

            _ = Drain(enumerator);

            enumerator.ValueDataLengths["Growing"].Should().Be(currentData.Length);
            enumerator.ValueChecksums["Growing"].Should().Be(Checksum(currentData));
        });
    }

    [TestMethod]
    public void MoveNext_RequestedTypedValues_ProjectManagedRegistryEncodings()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue(string.Empty, string.Empty);
            managedRoot.SetValue("Multi", new[] { "first", "second" });
            managedRoot.SetValue(
                "Dword",
                unchecked((int)0xFEDCBA98),
                global::Microsoft.Win32.RegistryValueKind.DWord);
            managedRoot.SetValue(
                "Qword",
                0x0123456789ABCDEFL,
                global::Microsoft.Win32.RegistryValueKind.QWord);

            using RecordingRegistryEnumerator enumerator = new(root) { ReadAllValues = true };
            _ = Drain(enumerator);

            enumerator.ValueStrings[string.Empty].Should().Equal(string.Empty);
            enumerator.ValueStrings["Multi"].Should().Equal("first", "second");
            enumerator.UInt32Values["Dword"].Should().Be(0xFEDCBA98);
            enumerator.UInt64Values["Dword"].Should().Be(0xFEDCBA98);
            enumerator.UInt64Values["Qword"].Should().Be(0x0123456789ABCDEF);
            enumerator.UInt32Values.Should().NotContainKey("Qword");
        });
    }

    [TestMethod]
    public void MoveNext_DeepTreeAndLongValueName_GrowTraversalBuffers()
    {
        WithTestKey((managedRoot, root) =>
        {
            string[] segments = new string[12];
            for (int index = 0; index < segments.Length; index++)
            {
                segments[index] = $"Level{index:D2}_{new string((char)('a' + index), 20)}";
            }

            string deepestPath = string.Join('\\', segments);
            string longValueName = new('v', 500);
            byte[] data = [1, 2, 3, 4];
            using global::Microsoft.Win32.RegistryKey deepest = managedRoot.CreateSubKey(deepestPath);
            deepest.SetValue(longValueName, data, global::Microsoft.Win32.RegistryValueKind.Binary);

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options) { ReadAllValues = true };

            string[] results = Drain(enumerator);

            results.Should().HaveCount(segments.Length + 1);
            results[^1].Should().Be($"V:{deepestPath}:{longValueName}:<binary>");
            enumerator.ValueDataLengths[longValueName].Should().Be(data.Length);
            enumerator.ValueChecksums[longValueName].Should().Be(Checksum(data));
            enumerator.FinishedPaths.Should().HaveCount(segments.Length + 1);
            enumerator.FinishedPaths[0].Should().Be(deepestPath);
            enumerator.FinishedPaths[^1].Should().BeEmpty();
        });
    }

    [TestMethod]
    public void MoveNext_PerformanceText_EnumeratesNamesWithoutReadingData()
    {
        using RecordingRegistryEnumerator enumerator = new(HKEY.HKEY_PERFORMANCE_TEXT);
        enumerator.SkipSubKeyPaths.Add(string.Empty);

        Drain(enumerator).Should().ContainInOrder(
            "V::Counter:<unread>",
            "V::Help:<unread>");
    }

    [TestMethod]
    public void ContinueOnError_InvalidRootAbandonsPhasesAndCompletes()
    {
        using RecordingRegistryEnumerator enumerator = new(new HKEY(new IntPtr(-1))) { ContinueErrors = true };

        enumerator.MoveNext().Should().BeFalse();

        enumerator.ErrorOperations.Should().Equal(
            RegistryEnumerationOperation.EnumerateValue,
            RegistryEnumerationOperation.EnumerateSubKey);
        enumerator.ErrorCodes.Should().OnlyContain(error => error == WIN32_ERROR.ERROR_INVALID_HANDLE);
        enumerator.FinishedPaths.Should().Equal(string.Empty);
    }

    [TestMethod]
    public void ContinueOnError_ValueRemovedBeforeReadPreservesEntryContext()
    {
        WithTestKey((managedRoot, root) =>
        {
            managedRoot.SetValue("Removed", "value");

            using RecordingRegistryEnumerator enumerator = new(root)
            {
                ContinueErrors = true,
                BeforeRead = name => managedRoot.DeleteValue(name),
            };
            enumerator.ReadValueNames.Add("Removed");

            Drain(enumerator).Should().BeEmpty();

            enumerator.ErrorOperations.Should().Equal(RegistryEnumerationOperation.ReadValueData);
            enumerator.ErrorCodes.Should().Equal(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
            enumerator.ErrorEntryNames.Should().Equal("Removed");
            enumerator.ErrorEntryKinds.Should().Equal(RegistryEntryKind.Value);
            enumerator.ErrorPaths.Should().Equal(string.Empty);
            enumerator.ErrorDepths.Should().Equal(0);
            enumerator.ErrorKeyResults.Should().Equal(WIN32_ERROR.ERROR_SUCCESS);
            enumerator.ContinueValueNames.Should().Equal("Removed");
        });
    }

    [TestMethod]
    public void ContinueOnError_SubKeyRemovedBeforeOpen_ReturnsDiscoveredKey()
    {
        WithTestKey((managedRoot, root) =>
        {
            using (managedRoot.CreateSubKey("Vanishing"))
            {
            }

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options)
            {
                ContinueErrors = true,
                BeforeRecurse = name => managedRoot.DeleteSubKeyTree(name),
            };

            Drain(enumerator).Should().Equal("K:Vanishing");

            enumerator.ErrorOperations.Should().Equal(RegistryEnumerationOperation.OpenSubKey);
            enumerator.ErrorCodes.Should().HaveCount(1);
            enumerator.ErrorCodes[0].Should().BeOneOf(
                WIN32_ERROR.ERROR_FILE_NOT_FOUND,
                WIN32_ERROR.ERROR_PATH_NOT_FOUND);
            enumerator.ErrorEntryKinds.Should().Equal(RegistryEntryKind.Key);
            enumerator.ErrorEntryNames.Should().Equal("Vanishing");
            enumerator.ErrorPaths.Should().Equal(string.Empty);
            enumerator.ErrorDepths.Should().Equal(1);
        });
    }

    [TestMethod]
    public void MoveNext_NativeError_DefaultBehaviorThrowsAndDisposesEnumerator()
    {
        using RecordingRegistryEnumerator enumerator = new(new HKEY(new IntPtr(-1)));

        Action moveNext = () => enumerator.MoveNext();
        moveNext.Should().Throw<Exception>();
        enumerator.MoveNext().Should().BeFalse();
    }

    [TestMethod]
    public void Dispose_ClosesOwnedDescendantAndLeavesRootBorrowed()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("ChildValue", "value");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            RecordingRegistryEnumerator enumerator = new(root, options);

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().Be("K:Child");
            enumerator.MoveNext().Should().BeTrue();
            HKEY descendant = enumerator.CapturedDescendantKey;
            descendant.IsNull.Should().BeFalse();

            enumerator.Dispose();
            enumerator.Dispose();

            PInvoke.RegQueryInfoKey(descendant, default).Should().Be(WIN32_ERROR.ERROR_INVALID_HANDLE);
            PInvoke.RegQueryInfoKey(root, default).Should().Be(WIN32_ERROR.ERROR_SUCCESS);
            enumerator.DisposeCount.Should().Be(1);
            enumerator.LastDisposeWasDisposing.Should().BeTrue();
            enumerator.MoveNext().Should().BeFalse();
        });
    }

    [TestMethod]
    public void MoveNext_CallbackException_ClosesOwnedDescendant()
    {
        WithTestKey((managedRoot, root) =>
        {
            using global::Microsoft.Win32.RegistryKey child = managedRoot.CreateSubKey("Child");
            child.SetValue("ChildValue", "value");

            RegistryEnumerationOptions options = new() { RecurseSubKeys = true };
            using RecordingRegistryEnumerator enumerator = new(root, options)
            {
                ThrowOnValuePath = "Child",
            };

            enumerator.MoveNext().Should().BeTrue();
            Action moveNext = () => enumerator.MoveNext();
            moveNext.Should().Throw<InvalidOperationException>();

            PInvoke.RegQueryInfoKey(enumerator.CapturedDescendantKey, default)
                .Should().Be(WIN32_ERROR.ERROR_INVALID_HANDLE);
            PInvoke.RegQueryInfoKey(root, default).Should().Be(WIN32_ERROR.ERROR_SUCCESS);
            enumerator.DisposeCount.Should().Be(1);
            enumerator.LastDisposeWasDisposing.Should().BeTrue();
            enumerator.MoveNext().Should().BeFalse();
        });
    }

    [TestMethod]
    public void Reset_ThrowsNotSupportedException()
    {
        WithTestKey((_, root) =>
        {
            using RecordingRegistryEnumerator enumerator = new(root);
            Action reset = enumerator.Reset;
            reset.Should().Throw<NotSupportedException>();
        });
    }

    [TestMethod]
    public void MaxRecursionDepth_Negative_ThrowsArgumentOutOfRangeException()
    {
        Action setDepth = () => _ = new RegistryEnumerationOptions { MaxRecursionDepth = -1 };
        setDepth.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Constructor_NullRoot_ThrowsArgumentException()
    {
        Action construct = () => _ = new RecordingRegistryEnumerator(default);
        construct.Should().Throw<ArgumentException>();
    }

    private static string[] Drain(RecordingRegistryEnumerator enumerator)
    {
        List<string> results = [];
        while (enumerator.MoveNext())
        {
            results.Add(enumerator.Current);
        }

        enumerator.MoveNext().Should().BeFalse();
        return [.. results];
    }

    private static void WithTestKey(Action<global::Microsoft.Win32.RegistryKey, HKEY> action)
    {
        string path = $@"Software\ThirtyTwo.Tests\RegistryEnumerator\{Guid.NewGuid():N}";

        try
        {
            using global::Microsoft.Win32.RegistryKey managedRoot =
                global::Microsoft.Win32.Registry.CurrentUser.CreateSubKey(path);
            using HKEY root = Registry.OpenKey(HKEY.HKEY_CURRENT_USER, path);
            action(managedRoot, root);
        }
        finally
        {
            global::Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
        }
    }

    private static uint Checksum(ReadOnlySpan<byte> data)
    {
        uint checksum = 0;
        foreach (byte value in data)
        {
            checksum = unchecked((checksum * 31) + value);
        }

        return checksum;
    }

    private sealed class RecordingRegistryEnumerator(
        HKEY root,
        RegistryEnumerationOptions? options = null) : RegistryEnumerator<string>(root, options)
    {
        internal HashSet<string> ReadValueNames { get; } = [];
        internal HashSet<string> ExcludedValueNames { get; } = [];
        internal HashSet<string> ExcludedKeyNames { get; } = [];
        internal HashSet<string> NonrecursiveKeyNames { get; } = [];
        internal HashSet<string> SkipValuePaths { get; } = [];
        internal HashSet<string> SkipSubKeyPaths { get; } = [];
        internal Dictionary<string, (bool HasData, string Value)> ValueStates { get; } = [];
        internal Dictionary<string, int> DeclaredDataLengths { get; } = [];
        internal Dictionary<string, int> ValueDataLengths { get; } = [];
        internal Dictionary<string, uint> ValueChecksums { get; } = [];
        internal Dictionary<string, string[]> ValueStrings { get; } = [];
        internal Dictionary<string, uint> UInt32Values { get; } = [];
        internal Dictionary<string, ulong> UInt64Values { get; } = [];
        internal Dictionary<string, int> KeyDepths { get; } = [];
        internal Dictionary<string, int> ValueDepths { get; } = [];
        internal HashSet<string> KeysWithWriteTimes { get; } = [];
        internal List<string> FinishedPaths { get; } = [];
        internal List<RegistryEnumerationOperation> ErrorOperations { get; } = [];
        internal List<WIN32_ERROR> ErrorCodes { get; } = [];
        internal List<string> ErrorEntryNames { get; } = [];
        internal List<RegistryEntryKind?> ErrorEntryKinds { get; } = [];
        internal List<string> ErrorPaths { get; } = [];
        internal List<int> ErrorDepths { get; } = [];
        internal List<WIN32_ERROR> ErrorKeyResults { get; } = [];
        internal List<string> ReadDecisionNames { get; } = [];
        internal List<string> ContinueValueNames { get; } = [];
        internal bool ReadAllValues { get; set; }
        internal bool ContinueErrors { get; set; }
        internal bool StopAfterValue { get; set; }
        internal string? ThrowOnValuePath { get; set; }
        internal Action<string>? BeforeRead { get; set; }
        internal Action<string>? BeforeRecurse { get; set; }
        internal HKEY CapturedDescendantKey { get; private set; }
        internal bool AllCallbackHandlesValid { get; private set; } = true;
        internal int DisposeCount { get; private set; }
        internal bool? LastDisposeWasDisposing { get; private set; }

        protected override bool ShouldEnumerateValues(ref RegistryKeyContext key)
        {
            CheckHandle(key.Key);
            if (key.Depth > 0)
            {
                CapturedDescendantKey = key.Key;
            }

            return !SkipValuePaths.Contains(key.RelativePath.ToString());
        }

        protected override bool ShouldEnumerateSubKeys(ref RegistryKeyContext key)
        {
            CheckHandle(key.Key);
            return !SkipSubKeyPaths.Contains(key.RelativePath.ToString());
        }

        protected override bool ShouldIncludeKey(ref RegistryKeyEntry key)
        {
            CheckHandle(key.ParentKey);
            string path = key.RelativePath.ToString();
            KeyDepths[path] = key.Depth;
            if (key.LastWriteTime.dwLowDateTime != 0 || key.LastWriteTime.dwHighDateTime != 0)
            {
                KeysWithWriteTimes.Add(path);
            }

            return !ExcludedKeyNames.Contains(key.Name.ToString());
        }

        protected override bool ShouldIncludeValue(ref RegistryValueEntry value)
        {
            CheckHandle(value.Key);
            string name = value.Name.ToString();
            ValueDepths[$"{value.KeyRelativePath}:{name}"] = value.Depth;
            return !ExcludedValueNames.Contains(name);
        }

        protected override bool ShouldRecurseIntoKey(ref RegistryKeyEntry key)
        {
            string name = key.Name.ToString();
            BeforeRecurse?.Invoke(name);
            return !NonrecursiveKeyNames.Contains(name);
        }

        protected override bool ShouldReadValueData(ref RegistryValueEntry value)
        {
            string name = value.Name.ToString();
            ReadDecisionNames.Add(name);
            bool read = ReadAllValues || ReadValueNames.Contains(name);
            if (read)
            {
                BeforeRead?.Invoke(name);
            }

            return read;
        }

        protected override bool ShouldContinueEnumeratingValues(ref RegistryValueEntry value)
        {
            ContinueValueNames.Add(value.Name.ToString());
            return !StopAfterValue;
        }

        protected override string TransformKey(ref RegistryKeyEntry key)
        {
            CheckHandle(key.ParentKey);
            return $"K:{key.RelativePath}";
        }

        protected override string TransformValue(ref RegistryValueEntry value)
        {
            CheckHandle(value.Key);
            string path = value.KeyRelativePath.ToString();
            if (path == ThrowOnValuePath)
            {
                throw new InvalidOperationException("Requested callback failure.");
            }

            string name = value.Name.ToString();
            string[] strings = value.HasData ? ReadStrings(value) : [];
            string projectedValue = !value.HasData
                ? "<unread>"
                : strings.Length == 0
                    ? "<binary>"
                    : strings[0];
            ValueStates[name] = (value.HasData, projectedValue);
            DeclaredDataLengths[name] = value.DataLength;
            ValueDataLengths[name] = value.Data.Length;
            ValueChecksums[name] = Checksum(value.Data);
            ValueStrings[name] = strings;

            if (value.TryReadUInt32(out uint uint32))
            {
                UInt32Values[name] = uint32;
            }

            if (value.TryReadUInt64(out ulong uint64))
            {
                UInt64Values[name] = uint64;
            }

            return $"V:{path}:{name}:{projectedValue}";
        }

        protected override void OnKeyFinished(ref RegistryKeyContext key)
            => FinishedPaths.Add(key.RelativePath.ToString());

        protected override bool ContinueOnError(ref RegistryEnumerationError error)
        {
            ErrorOperations.Add(error.Operation);
            ErrorCodes.Add(error.ErrorCode);
            ErrorEntryNames.Add(error.EntryName.ToString());
            ErrorEntryKinds.Add(error.EntryKind);
            ErrorPaths.Add(error.KeyRelativePath.ToString());
            ErrorDepths.Add(error.Depth);
            ErrorKeyResults.Add(PInvoke.RegQueryInfoKey(error.Key, default));
            return ContinueErrors;
        }

        protected override void Dispose(bool disposing)
        {
            DisposeCount++;
            LastDisposeWasDisposing = disposing;
        }

        private void CheckHandle(HKEY key)
            => AllCallbackHandlesValid &= PInvoke.RegQueryInfoKey(key, default) == WIN32_ERROR.ERROR_SUCCESS;

        private static string[] ReadStrings(RegistryValueEntry value)
        {
            List<string> strings = [];
            foreach (ReadOnlySpan<char> text in value.Strings)
            {
                strings.Add(text.ToString());
            }

            return [.. strings];
        }
    }
}