using Avalonia;
using System;
using System.Threading;
using Velopack;

namespace LanaDelSsh;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Must be first: handles install/update/uninstall hooks and exits early when needed
        VelopackApp.Build().Run();

        using var mutex = new Mutex(initiallyOwned: true, "Global\\LanaDelSsh-SingleInstance", out bool acquired);
        if (!acquired)
            return;

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
