using System;
using System.Windows;

namespace EliteSwitch;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch all unhandled exceptions
        DispatcherUnhandledException += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine("=== UNHANDLED EXCEPTION ===");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {args.Exception.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {args.Exception.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace:\n{args.Exception.StackTrace}");

            if (args.Exception.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {args.Exception.InnerException.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Inner Message: {args.Exception.InnerException.Message}");
            }

            // Don't mark as handled - let it show in the debugger
            args.Handled = false;
        };

        System.Diagnostics.Debug.WriteLine("App: OnStartup completed");
    }
}
