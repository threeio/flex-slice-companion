using System.Diagnostics;
using FlexSliceCompanion.Core.Apps;

namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed class WsjtxLauncher : IExternalRadioApp
{
    private readonly string _path;
    private readonly WsjtxConfigManager _configManager;
    private readonly Dictionary<string, Process> _processes = [];

    public WsjtxLauncher(string path, WsjtxConfigManager? configManager = null)
    {
        _path = path;
        _configManager = configManager ?? new WsjtxConfigManager();
    }

    public string Name => "WSJT-X";

    public bool IsInstalled() => !string.IsNullOrWhiteSpace(_path) && File.Exists(_path);

    public Task<AppLaunchResult> LaunchAsync(AppLaunchRequest request, CancellationToken cancellationToken = default)
    {
        _configManager.EnsureSettingsDirectory(request);
        var plan = _configManager.BuildLaunchPlan(_path, request);

        var startInfo = new ProcessStartInfo
        {
            FileName = plan.ExecutablePath,
            Arguments = plan.Arguments,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(plan.ExecutablePath) ?? Environment.CurrentDirectory
        };

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("WSJT-X process did not start.");
        _processes[plan.InstanceId] = process;

        return Task.FromResult(new AppLaunchResult
        {
            InstanceId = plan.InstanceId,
            ExecutablePath = plan.ExecutablePath,
            Arguments = plan.Arguments,
            ProcessId = process.Id
        });
    }

    public Task StopAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (_processes.TryGetValue(instanceId, out var process) && !process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }

        _processes.Remove(instanceId);
        return Task.CompletedTask;
    }
}
