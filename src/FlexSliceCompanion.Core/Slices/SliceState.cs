namespace FlexSliceCompanion.Core.Slices;

public sealed record SliceState
{
    public required string SliceId { get; init; }
    public required string Letter { get; init; }
    public double FrequencyHz { get; init; }
    public required string Mode { get; init; }
    public int? DaxChannel { get; init; }
    public bool IsTx { get; init; }
    public bool IsActive { get; init; }
    public string? PanadapterId { get; init; }
}
