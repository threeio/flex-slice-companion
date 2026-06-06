using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Radio;

public interface IRadioClient
{
    Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default);
    Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    IObservable<RadioState> RadioStateChanged { get; }
    IObservable<SliceState> SliceChanged { get; }

    Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default);
    Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default);
    Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default);
    Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default);
}
