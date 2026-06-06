namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed class WsjtxInstallLocator
{
    private static readonly string[] CommonWindowsPaths =
    [
        @"C:\WSJT\wsjtx\bin\wsjtx.exe",
        @"C:\WSJT\bin\wsjtx.exe",
        @"C:\Program Files\WSJT\wsjtx\bin\wsjtx.exe",
        @"C:\Program Files (x86)\WSJT\wsjtx\bin\wsjtx.exe"
    ];

    public string? Find(string configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        return CommonWindowsPaths.FirstOrDefault(File.Exists);
    }
}
