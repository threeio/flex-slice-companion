using System.Reactive.Subjects;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.FlexLib;

public sealed class FlexLibRadioClient : IRadioClient
{
    private readonly Subject<RadioState> _radioStateChanged = new();
    private readonly Subject<SliceState> _sliceChanged = new();

    public IObservable<RadioState> RadioStateChanged => _radioStateChanged;
    public IObservable<SliceState> SliceChanged => _sliceChanged;

    public Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        // FlexLib is intentionally isolated here. Add the FlexLib package/reference on Windows,
        // then map discovered Radio objects into RadioInfo.
        return Task.FromResult<IReadOnlyList<RadioInfo>>([]);
    }

    public Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default)
    {
        _radioStateChanged.OnNext(new RadioState { ConnectedRadio = radio });
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _radioStateChanged.OnNext(new RadioState());
        return Task.CompletedTask;
    }

    public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
