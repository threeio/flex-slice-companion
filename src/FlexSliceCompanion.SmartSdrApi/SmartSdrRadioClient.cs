using System.Reactive.Subjects;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrRadioClient : IRadioClient, IAsyncDisposable
{
    private readonly SmartSdrUdpDiscovery _discovery = new();
    private readonly Subject<RadioState> _radioStateChanged = new();
    private readonly Subject<SliceState> _sliceChanged = new();
    private SmartSdrTcpClient? _tcpClient;
    private RadioInfo? _connectedRadio;

    public IObservable<RadioState> RadioStateChanged => _radioStateChanged;
    public IObservable<SliceState> SliceChanged => _sliceChanged;

    public Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default) =>
        _discovery.DiscoverAsync(cancellationToken);

    public async Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(radio.IpAddress))
        {
            throw new InvalidOperationException("Radio discovery did not include an IP address.");
        }

        _tcpClient = new SmartSdrTcpClient();
        _tcpClient.SliceChanged += (_, slice) => _sliceChanged.OnNext(slice);
        await _tcpClient.ConnectAsync(radio.IpAddress, 4992, cancellationToken);
        _connectedRadio = radio;
        _radioStateChanged.OnNext(new RadioState { ConnectedRadio = radio });
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_tcpClient is not null)
        {
            await _tcpClient.DisposeAsync();
            _tcpClient = null;
        }

        _connectedRadio = null;
        _radioStateChanged.OnNext(new RadioState());
    }

    public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default) =>
        RequireClient().SetSliceFrequencyAsync(sliceId, frequencyHz, cancellationToken);

    public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default) =>
        RequireClient().SetSliceModeAsync(sliceId, mode, cancellationToken);

    public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default) =>
        RequireClient().SetTxSliceAsync(sliceId, cancellationToken);

    public Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default) =>
        RequireClient().SetPttAsync(isTransmit, cancellationToken);

    private SmartSdrTcpClient RequireClient() =>
        _tcpClient ?? throw new InvalidOperationException("No SmartSDR radio is connected.");

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _radioStateChanged.Dispose();
        _sliceChanged.Dispose();
    }
}
