namespace FlexSliceCompanion.Core.Cat;

public sealed record CatCommand
{
    public CatCommandKind Kind { get; init; }
    public double? FrequencyHz { get; init; }
    public string? Mode { get; init; }
    public bool? Ptt { get; init; }
    public string Raw { get; init; } = string.Empty;
}
