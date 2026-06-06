namespace FlexSliceCompanion.Core.Apps;

public interface IExternalRadioApp
{
    string Name { get; }
    bool IsInstalled();
    Task<AppLaunchResult> LaunchAsync(AppLaunchRequest request, CancellationToken cancellationToken = default);
    Task StopAsync(string instanceId, CancellationToken cancellationToken = default);
}
