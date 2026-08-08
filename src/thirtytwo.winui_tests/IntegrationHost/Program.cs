// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;
using Windows;
using Windows.WinUI;

namespace IntegrationHost;

internal static class Program
{
    [STAThread]
    private static int Main(string[] arguments)
    {
        EnvironmentScenario scenario;
        try
        {
            scenario = ScenarioArguments.Parse(arguments);
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }

        ScenarioReporter reporter = new(scenario);
        using XamlHostEventListener eventListener = new();
        reporter.Write("process-started");
        DispatcherQueueController? borrowedQueueController = null;
        WindowsXamlManager? existingManager = null;
        Microsoft.UI.Xaml.Application? existingApplication = null;
        WeakReference<Microsoft.UI.Xaml.Application>? environmentApplication = null;

        try
        {
            if (scenario is EnvironmentScenario.Borrowed
                or EnvironmentScenario.CompatibleApplication
                or EnvironmentScenario.IncompatibleApplication)
            {
                borrowedQueueController = DispatcherQueueController.CreateOnCurrentThread();
                reporter.Write("external-queue-created");
            }

            if (scenario is EnvironmentScenario.CompatibleApplication
                or EnvironmentScenario.IncompatibleApplication)
            {
                existingApplication = scenario == EnvironmentScenario.CompatibleApplication
                    ? new CompatibleApplication()
                    : new IncompatibleApplication();
                existingManager = WindowsXamlManager.InitializeForCurrentThread();
                if (existingApplication is CompatibleApplication compatibleApplication)
                {
                    compatibleApplication.InitializeComposition();
                }

                reporter.Write("external-application-created");
            }

            EnvironmentWindow? window = null;
            Application.Run(() =>
            {
                EnvironmentWindow createdWindow = new(scenario, reporter);
                window = createdWindow;
                environmentApplication = XamlHostEnvironment.Current is not null
                    ? new(Microsoft.UI.Xaml.Application.Current)
                    : existingApplication is null ? null : new(existingApplication);
                return createdWindow;
            });

            window?.VerifyAfterRun(scenario);
            Ensure(XamlHostEnvironment.Current is null, "The environment survived core dispatcher shutdown.");
            reporter.Write("environment-stopped");
            if (environmentApplication is not null)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Ensure(environmentApplication.TryGetTarget(out _), "The process application was not retained.");
                reporter.Write("application-retained");
            }

            if (borrowedQueueController is null)
            {
                Ensure(DispatcherQueue.GetForCurrentThread() is null, "An owned dispatcher queue remained active.");
                reporter.Write("owned-queue-stopped");
            }
            else
            {
                Ensure(DispatcherQueue.GetForCurrentThread() is not null, "A borrowed dispatcher queue was shut down.");
                reporter.Write("borrowed-queue-retained");
            }

            VerifyDiagnostics(scenario, eventListener.EventIds);
            reporter.Write("product-diagnostics-observed");
            reporter.Write("scenario-completed");
            GC.KeepAlive(existingApplication);
            return 0;
        }
        catch (Exception exception)
        {
            reporter.Write("scenario-failed", message: exception.ToString());
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            existingManager?.Dispose();
            borrowedQueueController?.ShutdownQueue();
        }
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void VerifyDiagnostics(EnvironmentScenario scenario, IReadOnlyList<int> eventIds)
    {
        if (scenario is EnvironmentScenario.IncompatibleApplication or EnvironmentScenario.MtaRejected)
        {
            Ensure(eventIds.Contains(4), "The initialization failure was not traced.");
            return;
        }

        Ensure(eventIds.Contains(1), "Environment creation was not traced.");
        Ensure(eventIds.Contains(2), "Lease changes were not traced.");
        Ensure(eventIds.Contains(3), "Environment shutdown was not traced.");

        if (scenario == EnvironmentScenario.Composition)
        {
            Ensure(eventIds.Contains(5), "Metadata collisions were not traced.");
            Ensure(eventIds.Contains(6), "Resource collisions were not traced.");
        }

        if (scenario == EnvironmentScenario.SecondThreadRejected)
        {
            Ensure(eventIds.Contains(4), "Second-thread rejection was not traced.");
        }
    }
}