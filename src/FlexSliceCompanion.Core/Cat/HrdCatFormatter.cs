using System.Globalization;

namespace FlexSliceCompanion.Core.Cat;

public sealed class HrdCatFormatter
{
    public string FormatFrequency(double frequencyHz) =>
        $"FA{frequencyHz.ToString("00000000000", CultureInfo.InvariantCulture)};";

    public string FormatMode(string mode) => $"MD{mode};";

    public string FormatPtt(bool ptt) => $"TX{(ptt ? 1 : 0)};";

    public string FormatOk() => "OK;";

    public string FormatUnknown() => "?;";
}
