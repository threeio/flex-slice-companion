namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed record WsjtxLaunchPlan
{
    public required string ExecutablePath { get; init; }
    public required string SettingsDirectory { get; init; }
    public required string Arguments { get; init; }
    public required string InstanceId { get; init; }
}
