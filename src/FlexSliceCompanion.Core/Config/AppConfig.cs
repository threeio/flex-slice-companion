namespace FlexSliceCompanion.Core.Config;

public sealed record AppConfig
{
    public RadioConfig Radio { get; init; } = new();
    public DaxConfig Dax { get; init; } = new();
    public AppsConfig Apps { get; init; } = new();

    public static AppConfig CreateDefault() => new();
}

public sealed record RadioConfig
{
    public string LastRadioSerial { get; init; } = string.Empty;
    public bool AutoConnect { get; init; } = true;
}

public sealed record DaxConfig
{
    public string PreferredGeneration { get; init; } = "Auto";
    public bool WarnIfBothDaxVersionsDetected { get; init; } = true;
    public bool IgnoreReservedEndpoints { get; init; } = true;
}

public sealed record AppsConfig
{
    public WsjtxConfig Wsjtx { get; init; } = new();
}

public sealed record WsjtxConfig
{
    public bool Enabled { get; init; } = true;
    public string Path { get; init; } = string.Empty;
    public bool AutoLaunch { get; init; } = true;
    public IReadOnlyList<string> LaunchWhenModeIn { get; init; } = ["DIGU", "DIGL"];
    public int BaseCatPort { get; init; } = 5100;
    public string SettingsRoot { get; init; } = "%APPDATA%/FlexSliceCompanion/wsjtx";
}
