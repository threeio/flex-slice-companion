namespace FlexSliceCompanion.Core.Audio;

public sealed record DaxScanResult(
    IReadOnlyList<DaxEndpoint> Endpoints,
    DaxGeneration PreferredGeneration,
    bool HasDaxV1,
    bool HasDaxV2,
    string? Warning,
    string? Message);
