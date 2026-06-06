using System.Reactive.Subjects;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Radio;

public sealed class DemoRadioClient : IRadioClient, IDisposable
{
    private readonly Subject<RadioState> _radioStateChanged = new();
    private readonly Subject<SliceState> _sliceChanged = new();
    private readonly Dictionary<string, SliceState> _slices = [];
    private RadioInfo? _connectedRadio;

    public IObservable<RadioState> RadioStateChanged => _radioStateChanged;
    public IObservable<SliceState> SliceChanged => _sliceChanged;

    public Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RadioInfo> radios =
        [
            new()
            {
                Id = "demo-flex-6400",
                Model = "FLEX-6400",
                Serial = "DEMO0001",
                Nickname = "Demo FLEX-6400",
                IpAddress = "127.0.0.1",
                SmartSdrVersion = new Version(4, 2, 18)
            }
        ];

        return Task.FromResult(radios);
    }

    public Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default)
    {
        _connectedRadio = radio;
        _radioStateChanged.OnNext(new RadioState { ConnectedRadio = radio });

        PublishSlice(new SliceState
        {
            SliceId = "0",
            Letter = "A",
            FrequencyHz = 14074000,
            Mode = "DIGU",
            DaxChannel = 1,
            IsTx = true,
            IsActive = true,
            PanadapterId = "demo-pan-0"
        });

        PublishSlice(new SliceState
        {
            SliceId = "1",
            Letter = "B",
            FrequencyHz = 7074000,
            Mode = "DIGU",
            DaxChannel = 2,
            IsTx = false,
            IsActive = false,
            PanadapterId = "demo-pan-0"
        });

        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _connectedRadio = null;
        _slices.Clear();
        _radioStateChanged.OnNext(new RadioState());
        return Task.CompletedTask;
    }

    public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default)
    {
        if (_slices.TryGetValue(sliceId, out var slice))
        {
            PublishSlice(slice with { FrequencyHz = frequencyHz });
        }

        return Task.CompletedTask;
    }

    public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default)
    {
        if (_slices.TryGetValue(sliceId, out var slice))
        {
            PublishSlice(slice with { Mode = mode.ToUpperInvariant() });
        }

        return Task.CompletedTask;
    }

    public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default)
    {
        foreach (var slice in _slices.Values.ToArray())
        {
            PublishSlice(slice with { IsTx = slice.SliceId == sliceId });
        }

        return Task.CompletedTask;
    }

    public Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default)
    {
        if (isTransmit)
        {
            return SetTxSliceAsync(sliceId, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private void PublishSlice(SliceState slice)
    {
        _slices[slice.SliceId] = slice;
        _sliceChanged.OnNext(slice);
    }

    public void Dispose()
    {
        _radioStateChanged.Dispose();
        _sliceChanged.Dispose();
    }
}
