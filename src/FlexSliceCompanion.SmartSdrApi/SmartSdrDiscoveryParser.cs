using FlexSliceCompanion.Core.Radio;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrDiscoveryParser
{
    public RadioInfo? Parse(ReadOnlySpan<byte> packet)
    {
        var text = ExtractAsciiStatus(packet);
        return string.IsNullOrWhiteSpace(text) ? null : Parse(text);
    }

    public RadioInfo? Parse(string status)
    {
        var values = ParseFields(status);
        if (!values.TryGetValue("model", out var model) ||
            !values.TryGetValue("serial", out var serial))
        {
            return null;
        }

        values.TryGetValue("name", out var name);
        values.TryGetValue("ip", out var ip);
        values.TryGetValue("version", out var versionText);

        return new RadioInfo
        {
            Id = serial,
            Model = model,
            Serial = serial,
            Nickname = string.IsNullOrWhiteSpace(name) ? model : name.Replace('_', ' '),
            IpAddress = ip,
            SmartSdrVersion = Version.TryParse(versionText, out var version) ? version : null
        };
    }

    private static Dictionary<string, string> ParseFields(string status)
    {
        return status
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
    }

    private static string? ExtractAsciiStatus(ReadOnlySpan<byte> packet)
    {
        const string marker = "model=";
        var text = System.Text.Encoding.ASCII.GetString(packet);
        var markerIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        return text[markerIndex..].TrimEnd('\0', '\r', '\n', ' ');
    }
}
