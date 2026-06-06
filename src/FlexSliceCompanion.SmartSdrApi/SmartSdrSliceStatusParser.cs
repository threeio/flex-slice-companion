using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrSliceStatusParser
{
    public SliceState? Parse(string status)
    {
        if (!status.Contains("slice ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sliceIndex = status.IndexOf("slice ", StringComparison.OrdinalIgnoreCase);
        var sliceStatus = status[(sliceIndex + "slice ".Length)..];
        var parts = sliceStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        var sliceId = parts[0];
        var values = parts
            .Skip(1)
            .Select(part => part.Split('=', 2))
            .Where(split => split.Length == 2)
            .ToDictionary(split => split[0], split => split[1], StringComparer.OrdinalIgnoreCase);

        values.TryGetValue("index_letter", out var letter);
        values.TryGetValue("mode", out var mode);
        values.TryGetValue("pan", out var pan);

        var frequencyHz = values.TryGetValue("RF_frequency", out var mhzText) &&
            double.TryParse(mhzText, out var mhz)
                ? mhz * 1_000_000d
                : 0;

        var daxChannel = values.TryGetValue("dax", out var daxText) &&
            int.TryParse(daxText, out var dax)
                ? dax
                : (int?)null;

        return new SliceState
        {
            SliceId = sliceId,
            Letter = string.IsNullOrWhiteSpace(letter) ? sliceId : letter,
            FrequencyHz = frequencyHz,
            Mode = string.IsNullOrWhiteSpace(mode) ? "USB" : mode.ToUpperInvariant(),
            DaxChannel = daxChannel,
            IsTx = values.TryGetValue("tx", out var tx) && tx == "1",
            IsActive = values.TryGetValue("active", out var active) && active == "1",
            PanadapterId = pan
        };
    }
}
