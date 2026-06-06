namespace FlexSliceCompanion.Windows.Services;

public static class ConfigPathResolver
{
    public static string GetUserConfigPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(root, "FlexSliceCompanion", "appsettings.json");
    }
}
