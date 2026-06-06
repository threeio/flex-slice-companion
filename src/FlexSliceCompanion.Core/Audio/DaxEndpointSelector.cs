namespace FlexSliceCompanion.Core.Audio;

public sealed class DaxEndpointSelector
{
    public DaxEndpointPair SelectForSlice(
        DaxScanResult scanResult,
        int? daxChannel,
        DaxGeneration preferredGeneration = DaxGeneration.Unknown)
    {
        ArgumentNullException.ThrowIfNull(scanResult);

        var assignable = new DaxEndpointDetector().GetAssignableEndpoints(scanResult);
        var preferred = preferredGeneration == DaxGeneration.Unknown
            ? scanResult.PreferredGeneration
            : preferredGeneration;

        var rx = FindEndpoint(assignable, DaxEndpointType.RxAudio, daxChannel, preferred)
            ?? throw new InvalidOperationException($"No DAX RX endpoint found for channel {daxChannel?.ToString() ?? "Auto"}.");

        var tx = FindEndpoint(assignable, DaxEndpointType.TxAudio, null, preferred)
            ?? throw new InvalidOperationException("No DAX TX endpoint found.");

        return new DaxEndpointPair(rx, tx);
    }

    private static DaxEndpoint? FindEndpoint(
        IReadOnlyList<DaxEndpoint> endpoints,
        DaxEndpointType type,
        int? channel,
        DaxGeneration preferredGeneration)
    {
        var candidates = endpoints.Where(endpoint => endpoint.Type == type);

        if (channel is not null)
        {
            candidates = candidates.Where(endpoint => endpoint.Channel == channel);
        }

        var ordered = candidates
            .OrderByDescending(endpoint => endpoint.Generation == preferredGeneration)
            .ThenBy(endpoint => endpoint.Channel ?? int.MaxValue)
            .ToArray();

        return ordered.FirstOrDefault();
    }
}
