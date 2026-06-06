using System.Net;
using System.Net.Sockets;
using System.Text;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;

namespace FlexSliceCompanion.Core.Cat;

public sealed class CatServer : IAsyncDisposable
{
    private readonly CatServerOptions _options;
    private readonly IRadioClient _radioClient;
    private readonly HrdCatParser _parser = new();
    private readonly HrdCatFormatter _formatter = new();
    private TcpListener? _listener;
    private SliceState? _slice;

    public CatServer(CatServerOptions options, IRadioClient radioClient)
    {
        _options = options;
        _radioClient = radioClient;
    }

    public Task UpdateSliceAsync(SliceState slice, CancellationToken cancellationToken = default)
    {
        if (slice.SliceId == _options.SliceId)
        {
            _slice = slice;
        }

        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener = new TcpListener(IPAddress.Loopback, _options.Port);
        _listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested || _listener is null)
        {
        }
    }

    public Task StopAsync()
    {
        _listener?.Stop();
        _listener = null;
        return Task.CompletedTask;
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var tcpClient = client;
        await using var stream = tcpClient.GetStream();

        var buffer = new byte[512];
        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read <= 0)
            {
                return;
            }

            var raw = Encoding.ASCII.GetString(buffer, 0, read);
            var response = await HandleCommandAsync(raw, cancellationToken);
            await stream.WriteAsync(Encoding.ASCII.GetBytes(response), cancellationToken);
        }
    }

    public async Task<string> HandleCommandAsync(string rawCommand, CancellationToken cancellationToken = default)
    {
        var command = _parser.Parse(rawCommand);

        switch (command.Kind)
        {
            case CatCommandKind.GetFrequency:
                return _slice is null ? _formatter.FormatUnknown() : _formatter.FormatFrequency(_slice.FrequencyHz);
            case CatCommandKind.SetFrequency when command.FrequencyHz is not null:
                await _radioClient.SetSliceFrequencyAsync(_options.SliceId, command.FrequencyHz.Value, cancellationToken);
                return _formatter.FormatOk();
            case CatCommandKind.GetMode:
                return _slice is null ? _formatter.FormatUnknown() : _formatter.FormatMode(_slice.Mode);
            case CatCommandKind.SetMode when !string.IsNullOrWhiteSpace(command.Mode):
                await _radioClient.SetSliceModeAsync(_options.SliceId, command.Mode, cancellationToken);
                return _formatter.FormatOk();
            case CatCommandKind.GetPtt:
                return _slice is null ? _formatter.FormatUnknown() : _formatter.FormatPtt(_slice.IsTx);
            case CatCommandKind.SetPtt when command.Ptt is not null:
                await _radioClient.SetPttAsync(_options.SliceId, command.Ptt.Value, cancellationToken);
                return _formatter.FormatOk();
            case CatCommandKind.Identify:
                return _formatter.FormatIdentifier();
            case CatCommandKind.AutoInformation:
                return _formatter.FormatOk();
            default:
                return _formatter.FormatUnknown();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
