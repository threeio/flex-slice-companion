using System.Windows;
using Serilog;

namespace FlexSliceCompanion.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var logRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlexSliceCompanion",
            "logs");
        Directory.CreateDirectory(logRoot);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logRoot, "FlexSliceCompanion-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("FlexSliceCompanion starting");
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("FlexSliceCompanion exiting");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
