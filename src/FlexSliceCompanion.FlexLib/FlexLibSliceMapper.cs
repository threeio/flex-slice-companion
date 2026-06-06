using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.FlexLib;

public static class FlexLibSliceMapper
{
    public static SliceState FromValues(
        string sliceId,
        string letter,
        double frequencyHz,
        string mode,
        int? daxChannel,
        bool isTx,
        bool isActive,
        string? panadapterId) =>
        new()
        {
            SliceId = sliceId,
            Letter = letter,
            FrequencyHz = frequencyHz,
            Mode = mode,
            DaxChannel = daxChannel,
            IsTx = isTx,
            IsActive = isActive,
            PanadapterId = panadapterId
        };
}
