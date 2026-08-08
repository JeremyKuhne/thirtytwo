// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Markup;
using SampleWinUIClassLibraryA;
using SampleWinUIClassLibraryB;
using Windows;
using Windows.Threading;
using Windows.Win32;
using Windows.WinUI;

namespace IntegrationHost;

internal sealed class EnvironmentWindow : Window
{
    private readonly ScenarioReporter _reporter;
    private XamlHostEnvironment? _environment;
    private XamlHostEnvironment? _secondEnvironment;

    internal EnvironmentWindow(EnvironmentScenario scenario, ScenarioReporter reporter)
        : base(DefaultBounds, text: "ThirtyTwo WinUI Environment Host")
    {
        _reporter = reporter;
        Execute(scenario);
        reporter.Write("ready", Handle);

        if (!Dispatcher.TryPost(() =>
            {
                if (!PInvoke.DestroyWindow(Handle))
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }
            }))
        {
            throw new InvalidOperationException("Failed to schedule environment host shutdown.");
        }
    }

    private void Execute(EnvironmentScenario scenario)
    {
        switch (scenario)
        {
            case EnvironmentScenario.Owned:
            case EnvironmentScenario.Borrowed:
            case EnvironmentScenario.CompatibleApplication:
                AcquireAndReport();
                break;
            case EnvironmentScenario.Composition:
                AcquireAndReport();
                VerifyComposition();
                break;
            case EnvironmentScenario.MultipleLeases:
                AcquireAndReport();
                _secondEnvironment = XamlHostEnvironment.Acquire();
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 2, "Two leases were not recorded.");
                _reporter.Write("lease-count-two");
                _environment!.Dispose();
                _environment = null;
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "One lease was not retained.");
                _reporter.Write("lease-count-one");
                break;
            case EnvironmentScenario.IncompatibleApplication:
                VerifyIncompatibleApplication();
                break;
            case EnvironmentScenario.MtaRejected:
                VerifyMtaRejected();
                break;
            case EnvironmentScenario.WrongThreadRejected:
                AcquireAndReport();
                VerifyWrongThreadRejected();
                break;
            case EnvironmentScenario.SecondThreadRejected:
                AcquireAndReport();
                VerifySecondThreadRejected();
                break;
            case EnvironmentScenario.FinalRelease:
                AcquireAndReport();
                WeakReference<Microsoft.UI.Xaml.Application> application = new(_environment!.Application);
                XamlHostEnvironment releasedEnvironment = _environment;
                releasedEnvironment.Dispose();
                releasedEnvironment.Dispose();
                _environment = null;
                Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "The final public lease was not released.");
                _reporter.Write("double-dispose-idempotent");
                _reporter.Write("final-lease-released");
                using (XamlHostEnvironment reacquired = XamlHostEnvironment.Acquire())
                {
                    Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The environment was not reacquired.");
                    Ensure(
                        application.TryGetTarget(out Microsoft.UI.Xaml.Application? target)
                        && ReferenceEquals(target, reacquired.Application),
                        "Reacquisition replaced the process application.");
                    _reporter.Write("environment-reacquired");
                }

                Ensure(XamlHostEnvironment.Current?.LeaseCount == 0, "The reacquired lease was not released.");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Ensure(application.TryGetTarget(out _), "The process application was not retained.");
                _reporter.Write("application-retained");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }
    }

    private void AcquireAndReport()
    {
        _environment = XamlHostEnvironment.Acquire();
        _reporter.Write(_environment.OwnsDispatcherQueue ? "queue-owned" : "queue-borrowed");
        _reporter.Write(_environment.OwnsApplication ? "application-owned" : "application-borrowed");
        Ensure(XamlHostEnvironment.Current?.LeaseCount == 1, "The first lease was not recorded.");
        _reporter.Write("environment-acquired");
    }

    private void VerifyComposition()
    {
        XamlMetadataProviderRegistry metadata = _environment!.MetadataProviders;
        XamlResourceDictionaryRegistry resources = _environment.ResourceDictionaries;
        int metadataCollisions = 0;
        int resourceCollisions = 0;
        metadata.CollisionDetected += (_, _) => metadataCollisions++;

        LibraryAMetadataProvider providerA = new();
        LibraryBMetadataProvider providerB = new();
        Ensure(metadata.Register(providerA), "Provider A was not registered.");
        Ensure(!metadata.Register(new LibraryAMetadataProvider()), "Provider A registration was not idempotent.");
        Ensure(metadata.Register(providerB), "Provider B was not registered.");

        IXamlType typeA = metadata.GetXamlType(typeof(LibraryAControl))
            ?? throw new InvalidOperationException("Provider A did not resolve its custom type.");
        IXamlType typeB = metadata.GetXamlType(typeof(LibraryBControl))
            ?? throw new InvalidOperationException("Provider B did not resolve its custom type.");
        Ensure(typeA.ActivateInstance() is LibraryAControl, "Provider A activated the wrong type.");
        Ensure(typeB.ActivateInstance() is LibraryBControl, "Provider B activated the wrong type.");
        IXamlType collision = metadata.GetXamlType(LibraryAMetadataProvider.CollisionTypeName)
            ?? throw new InvalidOperationException("The collision alias was not resolved.");
        Ensure(collision.UnderlyingType == typeof(LibraryAControl), "Metadata lookup did not preserve first-provider precedence.");
        Ensure(metadataCollisions == 1, "The metadata collision was not reported exactly once.");
        _reporter.Write("metadata-composed");
        _reporter.Write("metadata-collision-reported");

        LibraryAResources resourcesA = new();
        LibraryBResources resourcesB = new();
        Ensure(resources.Register(resourcesA), "Resources A were not registered.");
        Ensure(!resources.Register(resourcesA), "Resource registration was not idempotent.");
        EventHandler<XamlResourceCollisionEventArgs> throwingHandler =
            static (_, _) => throw new InvalidOperationException("Expected collision callback failure.");
        resources.CollisionDetected += throwingHandler;
        try
        {
            _ = resources.Register(resourcesB);
            throw new InvalidOperationException("A throwing collision callback did not fail registration.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "Expected collision callback failure.")
        {
            _reporter.Write("resource-registration-rolled-back");
        }
        finally
        {
            resources.CollisionDetected -= throwingHandler;
        }

        resources.CollisionDetected += (_, _) => resourceCollisions++;
        Ensure(resources.Register(resourcesB), "Resources B were not registered.");
        Ensure(resourceCollisions == 1, "The resource collision was not reported exactly once.");
        Ensure(
            Equals(_environment.Application.Resources[LibraryAResources.SharedResourceKey], "LibraryB"),
            "Later resource registration did not win duplicate-key lookup.");
        Ensure(resourcesA.ThemeDictionaries.ContainsKey("Default"), "Resources A lost its theme dictionary.");
        Ensure(resourcesB.ThemeDictionaries.ContainsKey("Default"), "Resources B lost its theme dictionary.");
        _reporter.Write("resources-composed");
        _reporter.Write("resource-collision-reported");
        _reporter.Write("theme-dictionaries-preserved");
    }

    private void VerifyIncompatibleApplication()
    {
        try
        {
            _ = XamlHostEnvironment.Acquire();
            throw new InvalidOperationException("An incompatible Application.Current was accepted.");
        }
        catch (XamlHostInitializationException exception) when (
            exception.Stage == XamlHostInitializationStage.Application)
        {
            _reporter.Write("incompatible-application-rejected", message: exception.Message);
        }
    }

    private void VerifyMtaRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = XamlHostEnvironment.Acquire();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        thread.Join();
        if (failure is not XamlHostInitializationException
            {
                Stage: XamlHostInitializationStage.ThreadValidation
            } diagnostic)
        {
            throw new InvalidOperationException("MTA acquisition did not fail with thread diagnostics.", failure);
        }

        Ensure(diagnostic.ManagedThreadId == thread.ManagedThreadId, "The MTA diagnostic did not capture its managed thread.");
        Ensure(diagnostic.NativeThreadId != 0, "The MTA diagnostic did not capture its native thread.");
        _reporter.Write("mta-rejected", message: diagnostic.Message);
    }

    private void VerifyWrongThreadRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _environment!.Dispose();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.Start();
        thread.Join();
        if (failure is not InvalidOperationException diagnostic)
        {
            throw new InvalidOperationException("Wrong-thread disposal was not rejected.", failure);
        }

        _reporter.Write("wrong-thread-rejected", message: diagnostic.Message);
    }

    private void VerifySecondThreadRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = XamlHostEnvironment.Acquire();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not XamlHostInitializationException
            {
                Stage: XamlHostInitializationStage.ThreadValidation
            } diagnostic)
        {
            throw new InvalidOperationException("A second XAML thread was not rejected.", failure);
        }

        _reporter.Write("second-thread-rejected", message: diagnostic.Message);
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _secondEnvironment?.Dispose();
            _environment?.Dispose();
        }

        base.Dispose(disposing);
    }
}