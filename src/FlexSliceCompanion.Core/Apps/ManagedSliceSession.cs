using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Apps;

public sealed record ManagedSliceSession
{
    public required string SessionId { get; init; }
    public required SliceState Slice { get; init; }
    public int CatPort { get; init; }
    public required DaxEndpointPair DaxEndpoints { get; init; }
    public AppLaunchResult? AppLaunch { get; init; }
}
