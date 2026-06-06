namespace FlexSliceCompanion.Core.Cat;

public sealed record CatServerOptions
{
    public required string SliceId { get; init; }
    public int Port { get; init; }
}
