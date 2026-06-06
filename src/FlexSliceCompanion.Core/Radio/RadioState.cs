namespace FlexSliceCompanion.Core.Radio;

public sealed record RadioState
{
    public RadioInfo? ConnectedRadio { get; init; }
    public bool IsConnected => ConnectedRadio is not null;
}
