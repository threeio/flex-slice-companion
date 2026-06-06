using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Apps;

public sealed record SliceSessionRequest
{
    public required SliceState Slice { get; init; }
    public required DaxScanResult DaxScanResult { get; init; }
    public required string SettingsRoot { get; init; }
    public int BaseCatPort { get; init; } = 5100;
    public DaxGeneration PreferredDaxGeneration { get; init; } = DaxGeneration.Unknown;
}
