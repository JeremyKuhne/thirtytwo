using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace DirectEditorDragRepro;

internal static class Program
{
    private static ReproApplication? s_application;

    [STAThread]
    private static void Main()
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(applicationInitializationCallbackParams =>
        {
            _ = applicationInitializationCallbackParams;
            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherQueueSynchronizationContext(dispatcherQueue));
            s_application = new ReproApplication();
        });
    }
}