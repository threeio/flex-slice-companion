namespace FlexSliceCompanion.Core.Audio;

public sealed record DaxEndpoint
{
    public required string WindowsDeviceId { get; init; }
    public required string DisplayName { get; init; }
    public DaxGeneration Generation { get; init; }
    public DaxEndpointType Type { get; init; }
    public int? Channel { get; init; }
    public bool IsReserved { get; init; }
}
