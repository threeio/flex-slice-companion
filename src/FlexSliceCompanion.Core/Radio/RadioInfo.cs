namespace FlexSliceCompanion.Core.Radio;

public sealed record RadioInfo
{
    public required string Id { get; init; }
    public required string Model { get; init; }
    public required string Serial { get; init; }
    public required string Nickname { get; init; }
    public string? IpAddress { get; init; }
    public Version? SmartSdrVersion { get; init; }
}
