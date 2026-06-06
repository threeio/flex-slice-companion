using System.Text.Json;

namespace FlexSliceCompanion.Core.Config;

public sealed class JsonConfigStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<AppConfig> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return AppConfig.CreateDefault();
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AppConfig>(stream, SerializerOptions, cancellationToken)
            ?? AppConfig.CreateDefault();
    }

    public async Task SaveAsync(string path, AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, config, SerializerOptions, cancellationToken);
    }
}
