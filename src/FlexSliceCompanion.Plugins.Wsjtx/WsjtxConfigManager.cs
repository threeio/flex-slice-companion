using FlexSliceCompanion.Core.Apps;

namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed class WsjtxConfigManager
{
    public string ResolveSettingsDirectory(string root, string sliceLetter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(sliceLetter);

        var expandedRoot = root.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase);
        return Path.Combine(expandedRoot, $"slice-{sliceLetter.ToUpperInvariant()}");
    }

    public void EnsureSettingsDirectory(AppLaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Directory.CreateDirectory(request.SettingsDirectory);
    }

    public WsjtxLaunchPlan BuildLaunchPlan(string executablePath, AppLaunchRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(request);

        var instanceId = $"wsjtx-slice-{request.Slice.Letter.ToUpperInvariant()}";
        var arguments = $"--rig-name={Quote(instanceId)} --config-path={Quote(request.SettingsDirectory)}";

        return new WsjtxLaunchPlan
        {
            ExecutablePath = executablePath,
            SettingsDirectory = request.SettingsDirectory,
            Arguments = arguments,
            InstanceId = instanceId
        };
    }

    private static string Quote(string value) => $"\"{value}\"";
}
