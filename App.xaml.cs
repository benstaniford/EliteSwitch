using System;
using System.Windows;

namespace EliteSwitch;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== APP STARTING ===");
        System.Diagnostics.Debug.WriteLine($"ShutdownMode: {this.ShutdownMode}");

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

        this.Activated += (s, args) =>
        {
            System.Diagnostics.Debug.WriteLine("App: Activated event");
        };

        this.Deactivated += (s, args) =>
        {
            System.Diagnostics.Debug.WriteLine("App: Deactivated event");
        };

        this.Exit += (s, args) =>
        {
            System.Diagnostics.Debug.WriteLine("App: Exit event");
        };

        System.Diagnostics.Debug.WriteLine("App: OnStartup completed");
        System.Diagnostics.Debug.WriteLine($"App.Windows.Count: {this.Windows.Count}");

        if (this.Windows.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow type: {this.MainWindow?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"MainWindow.IsLoaded: {this.MainWindow?.IsLoaded}");
        }
    }
}
