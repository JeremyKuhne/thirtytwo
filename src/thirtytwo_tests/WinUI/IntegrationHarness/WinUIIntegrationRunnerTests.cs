// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
public class WinUIIntegrationRunnerTests
{
    [TestMethod]
    public void WaitForCleanupAsync_IncompleteTask_RecordsTimeout()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<string> cleanupErrors = [];

        bool completed = WinUIIntegrationRunner.WaitForCleanupAsync(
            completion.Task,
            TimeSpan.FromMilliseconds(10),
            "Expected operation",
            cleanupErrors).GetAwaiter().GetResult();

        completed.Should().BeFalse();
        cleanupErrors.Should().ContainSingle().Which.Should().Contain("Expected operation did not complete");
    }

    [TestMethod]
    public void GetExitCode_RunningProcess_ReturnsNull()
    {
        using Process process = Process.GetCurrentProcess();

        int? exitCode = WinUIIntegrationRunner.GetExitCode(process);

        exitCode.Should().BeNull();
    }

    [TestMethod]
    public void WaitForCleanupAsync_FaultedTask_RecordsFailure()
    {
        List<string> cleanupErrors = [];
        Task task = Task.FromException(new InvalidOperationException("Expected"));

        bool completed = WinUIIntegrationRunner.WaitForCleanupAsync(
            task,
            TimeSpan.FromSeconds(1),
            "Expected operation",
            cleanupErrors).GetAwaiter().GetResult();

        completed.Should().BeFalse();
        cleanupErrors.Should().ContainSingle().Which.Should().Contain("Expected operation failed");
    }

    [TestMethod]
    public void GetExitCode_ExitedProcess_ReturnsExitCode()
    {
        using Process process = Process.Start(new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe",
            Arguments = "/c exit 7",
            CreateNoWindow = true,
            UseShellExecute = false
        }) ?? throw new InvalidOperationException("Failed to start the exit-code test process.");
        process.WaitForExit();

        int? exitCode = WinUIIntegrationRunner.GetExitCode(process);

        exitCode.Should().Be(7);
    }
}
