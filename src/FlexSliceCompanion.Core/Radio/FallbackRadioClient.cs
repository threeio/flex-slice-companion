using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Radio;

public sealed class FallbackRadioClient : IRadioClient
{
    private readonly IRadioClient _primary;
    private readonly IRadioClient _fallback;
    private IRadioClient? _active;

    public FallbackRadioClient(IRadioClient primary, IRadioClient fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public IObservable<RadioState> RadioStateChanged =>
        new MergedObservable<RadioState>(_primary.RadioStateChanged, _fallback.RadioStateChanged);

    public IObservable<SliceState> SliceChanged =>
        new MergedObservable<SliceState>(_primary.SliceChanged, _fallback.SliceChanged);

    public async Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var radios = await _primary.DiscoverAsync(cancellationToken);
        if (radios.Count > 0)
        {
            _active = _primary;
            return radios;
        }

        _active = _fallback;
        return await _fallback.DiscoverAsync(cancellationToken);
    }

    public Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default) =>
        Active.ConnectAsync(radio, cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        Active.DisconnectAsync(cancellationToken);

    public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default) =>
        Active.SetSliceFrequencyAsync(sliceId, frequencyHz, cancellationToken);

    public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default) =>
        Active.SetSliceModeAsync(sliceId, mode, cancellationToken);

    public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default) =>
        Active.SetTxSliceAsync(sliceId, cancellationToken);

    public Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default) =>
        Active.SetPttAsync(sliceId, isTransmit, cancellationToken);

    private IRadioClient Active => _active ?? _primary;

    private sealed class MergedObservable<T> : IObservable<T>
    {
        private readonly IObservable<T> _first;
        private readonly IObservable<T> _second;

        public MergedObservable(IObservable<T> first, IObservable<T> second)
        {
            _first = first;
            _second = second;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var first = _first.Subscribe(observer);
            var second = _second.Subscribe(observer);
            return new CompositeDisposable(first, second);
        }
    }

    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IDisposable _first;
        private readonly IDisposable _second;

        public CompositeDisposable(IDisposable first, IDisposable second)
        {
            _first = first;
            _second = second;
        }

        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }
    }
}
