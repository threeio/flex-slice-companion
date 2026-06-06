using System.Globalization;

namespace FlexSliceCompanion.Core.Cat;

public sealed class HrdCatParser
{
    public CatCommand Parse(string rawCommand)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawCommand);

        var raw = rawCommand.Trim();
        var command = raw.TrimEnd(';').Trim();

        if (command.Equals("FA", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.GetFrequency, Raw = raw };
        }

        if (command.StartsWith("FA", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(command[2..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var frequencyHz))
        {
            return new CatCommand { Kind = CatCommandKind.SetFrequency, FrequencyHz = frequencyHz, Raw = raw };
        }

        if (command.Equals("MD", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.GetMode, Raw = raw };
        }

        if (command.StartsWith("MD", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.SetMode, Mode = command[2..].Trim(), Raw = raw };
        }

        if (command.Equals("TX", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.GetPtt, Raw = raw };
        }

        if (command.StartsWith("TX", StringComparison.OrdinalIgnoreCase))
        {
            var value = command[2..].Trim();
            return value switch
            {
                "0" => new CatCommand { Kind = CatCommandKind.SetPtt, Ptt = false, Raw = raw },
                "1" => new CatCommand { Kind = CatCommandKind.SetPtt, Ptt = true, Raw = raw },
                _ => new CatCommand { Kind = CatCommandKind.Unknown, Raw = raw }
            };
        }

        if (command.Equals("ID", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.Identify, Raw = raw };
        }

        if (command.StartsWith("AI", StringComparison.OrdinalIgnoreCase))
        {
            return new CatCommand { Kind = CatCommandKind.AutoInformation, Raw = raw };
        }

        return new CatCommand { Kind = CatCommandKind.Unknown, Raw = raw };
    }
}
