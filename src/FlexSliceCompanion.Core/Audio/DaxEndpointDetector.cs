using System.Text.RegularExpressions;

namespace FlexSliceCompanion.Core.Audio;

public sealed partial class DaxEndpointDetector
{
    public DaxScanResult Detect(
        IEnumerable<WindowsAudioDevice> devices,
        Version? smartSdrVersion = null,
        DaxGeneration preferredGeneration = DaxGeneration.Unknown)
    {
        ArgumentNullException.ThrowIfNull(devices);

        var endpoints = devices
            .Select(Parse)
            .Where(endpoint => endpoint.Type != DaxEndpointType.Unknown)
            .OrderBy(endpoint => endpoint.Generation)
            .ThenBy(endpoint => endpoint.Type)
            .ThenBy(endpoint => endpoint.Channel ?? 0)
            .ToArray();

        var hasV1 = endpoints.Any(endpoint => endpoint.Generation == DaxGeneration.DaxV1);
        var hasV2 = endpoints.Any(endpoint => endpoint.Generation == DaxGeneration.DaxV2);
        var resolvedPreferred = ResolvePreferredGeneration(preferredGeneration, smartSdrVersion, hasV1, hasV2);

        string? warning = hasV1 && hasV2
            ? "Both DAXv1 and DAXv2 devices were detected. Old SmartSDR/DAX remnants may still be installed."
            : null;

        string? message = endpoints.Length == 0
            ? "No DAX devices are visible. With DAXv2, start SmartSDR DAX and refresh devices."
            : null;

        return new DaxScanResult(endpoints, resolvedPreferred, hasV1, hasV2, warning, message);
    }

    public IReadOnlyList<DaxEndpoint> GetAssignableEndpoints(DaxScanResult scanResult)
    {
        ArgumentNullException.ThrowIfNull(scanResult);

        return scanResult.Endpoints
            .Where(endpoint => !endpoint.IsReserved && endpoint.Type != DaxEndpointType.Reserved)
            .ToArray();
    }

    public DaxEndpoint Parse(WindowsAudioDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var displayName = device.DisplayName.Trim();

        if (DaxV2RxRegex().Match(displayName) is { Success: true } v2Rx)
        {
            return Endpoint(device, DaxGeneration.DaxV2, DaxEndpointType.RxAudio, Channel(v2Rx));
        }

        if (DaxV2IqRegex().Match(displayName) is { Success: true } v2Iq)
        {
            return Endpoint(device, DaxGeneration.DaxV2, DaxEndpointType.IqAudio, Channel(v2Iq));
        }

        if (DaxV2TxRegex().IsMatch(displayName))
        {
            return Endpoint(device, DaxGeneration.DaxV2, DaxEndpointType.TxAudio);
        }

        if (DaxV2MicRegex().IsMatch(displayName))
        {
            return Endpoint(device, DaxGeneration.DaxV2, DaxEndpointType.MicAudio);
        }

        if (DaxV1ReservedRxRegex().Match(displayName) is { Success: true } reservedRx)
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.Reserved, Channel(reservedRx), isReserved: true);
        }

        if (DaxV1ReservedIqRegex().Match(displayName) is { Success: true } reservedIq)
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.Reserved, Channel(reservedIq), isReserved: true);
        }

        if (DaxV1ReservedTxRegex().IsMatch(displayName) || DaxV1ReservedMicRegex().IsMatch(displayName))
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.Reserved, isReserved: true);
        }

        if (DaxV1RxRegex().Match(displayName) is { Success: true } v1Rx)
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.RxAudio, Channel(v1Rx));
        }

        if (DaxV1IqRegex().Match(displayName) is { Success: true } v1Iq)
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.IqAudio, Channel(v1Iq));
        }

        if (DaxV1TxRegex().IsMatch(displayName))
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.TxAudio);
        }

        if (DaxV1MicRegex().IsMatch(displayName))
        {
            return Endpoint(device, DaxGeneration.DaxV1, DaxEndpointType.MicAudio);
        }

        return Endpoint(device, DaxGeneration.Unknown, DaxEndpointType.Unknown);
    }

    private static DaxGeneration ResolvePreferredGeneration(
        DaxGeneration requested,
        Version? smartSdrVersion,
        bool hasV1,
        bool hasV2)
    {
        if (requested is DaxGeneration.DaxV1 or DaxGeneration.DaxV2)
        {
            return requested;
        }

        if (smartSdrVersion is not null && smartSdrVersion >= new Version(4, 2, 18) && hasV2)
        {
            return DaxGeneration.DaxV2;
        }

        if (hasV2)
        {
            return DaxGeneration.DaxV2;
        }

        return hasV1 ? DaxGeneration.DaxV1 : DaxGeneration.Unknown;
    }

    private static DaxEndpoint Endpoint(
        WindowsAudioDevice device,
        DaxGeneration generation,
        DaxEndpointType type,
        int? channel = null,
        bool isReserved = false) =>
        new()
        {
            WindowsDeviceId = device.Id,
            DisplayName = device.DisplayName,
            Generation = generation,
            Type = type,
            Channel = channel,
            IsReserved = isReserved || type == DaxEndpointType.Reserved
        };

    private static int? Channel(Match match) =>
        int.TryParse(match.Groups["channel"].Value, out var channel) ? channel : null;

    [GeneratedRegex(@"^DAX RX (?<channel>\d+)(?: \(FlexRadio DAX\))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV2RxRegex();

    [GeneratedRegex(@"^DAX IQ (?<channel>\d+)(?: \(FlexRadio DAX\))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV2IqRegex();

    [GeneratedRegex(@"^DAX TX(?: \(FlexRadio DAX\))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV2TxRegex();

    [GeneratedRegex(@"^DAX Mic(?: \(FlexRadio DAX\))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV2MicRegex();

    [GeneratedRegex(@"^DAX Audio RX (?<channel>\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1RxRegex();

    [GeneratedRegex(@"^DAX IQ RX (?<channel>\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1IqRegex();

    [GeneratedRegex(@"^DAX Audio TX$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1TxRegex();

    [GeneratedRegex(@"^DAX Mic Audio$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1MicRegex();

    [GeneratedRegex(@"^DAX RESERVED AUDIO RX (?<channel>\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1ReservedRxRegex();

    [GeneratedRegex(@"^DAX RESERVED IQ RX (?<channel>\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1ReservedIqRegex();

    [GeneratedRegex(@"^DAX RESERVED AUDIO TX$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1ReservedTxRegex();

    [GeneratedRegex(@"^DAX RESERVED MIC AUDIO$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DaxV1ReservedMicRegex();
}
