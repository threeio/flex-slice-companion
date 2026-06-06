namespace FlexSliceCompanion.Core.Apps;

public sealed record AppLaunchResult
{
    public required string InstanceId { get; init; }
    public required string ExecutablePath { get; init; }
    public string? Arguments { get; init; }
    public int? ProcessId { get; init; }
}
