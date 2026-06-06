using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Apps;

public sealed record AppLaunchRequest
{
    public required SliceState Slice { get; init; }
    public int CatPort { get; init; }
    public required DaxEndpoint RxAudioEndpoint { get; init; }
    public required DaxEndpoint TxAudioEndpoint { get; init; }
    public required string SettingsDirectory { get; init; }
}
