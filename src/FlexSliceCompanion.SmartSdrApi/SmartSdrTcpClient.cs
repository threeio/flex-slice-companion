using System.Globalization;
using System.Net.Sockets;
using System.Text;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrTcpClient : IAsyncDisposable
{
    private readonly SmartSdrSliceStatusParser _sliceStatusParser = new();
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private int _sequence;

    public event EventHandler<SliceState>? SliceChanged;

    public async Task ConnectAsync(string host, int port = 4992, CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(host, port, cancellationToken);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.ASCII);
        _writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\n" };
        _ = Task.Run(() => ReadLoopAsync(cancellationToken), cancellationToken);
    }

    public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default)
    {
        var mhz = frequencyHz / 1_000_000d;
        return SendCommandAsync($"slice tune {sliceId} {mhz.ToString("0.000000", CultureInfo.InvariantCulture)}", cancellationToken);
    }

    public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default) =>
        SendCommandAsync($"slice set {sliceId} mode={mode.ToLowerInvariant()}", cancellationToken);

    public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default) =>
        SendCommandAsync($"slice set {sliceId} tx=1", cancellationToken);

    public Task SetPttAsync(bool isTransmit, CancellationToken cancellationToken = default) =>
        SendCommandAsync($"xmit {(isTransmit ? 1 : 0)}", cancellationToken);

    private async Task SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("SmartSDR TCP client is not connected.");
        }

        var sequence = Interlocked.Increment(ref _sequence);
        await _writer.WriteLineAsync($"C{sequence}|{command}".AsMemory(), cancellationToken);
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        if (_reader is null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await _reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            var slice = _sliceStatusParser.Parse(line);
            if (slice is not null)
            {
                SliceChanged?.Invoke(this, slice);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _client?.Dispose();
        return ValueTask.CompletedTask;
    }
}
