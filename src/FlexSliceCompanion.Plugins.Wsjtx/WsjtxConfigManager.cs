using FlexSliceCompanion.Core.Apps;

namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed class WsjtxConfigManager : IAppSettingsDirectoryResolver
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
        WriteCompanionSettings(request);
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

    private static void WriteCompanionSettings(AppLaunchRequest request)
    {
        var path = Path.Combine(request.SettingsDirectory, "FlexSliceCompanion.wsjtx.ini");
        var lines = new[]
        {
            "[FlexSliceCompanion]",
            $"Slice={request.Slice.Letter}",
            $"SliceId={request.Slice.SliceId}",
            $"FrequencyHz={request.Slice.FrequencyHz:0}",
            $"Mode={request.Slice.Mode}",
            $"CatHost=127.0.0.1",
            $"CatPort={request.CatPort}",
            $"RxAudio={request.RxAudioEndpoint.DisplayName}",
            $"TxAudio={request.TxAudioEndpoint.DisplayName}",
            string.Empty,
            "; WSJT-X stores its own settings in this per-slice directory.",
            "; This companion file records the values FlexSliceCompanion selected."
        };

        File.WriteAllLines(path, lines);
    }
}
