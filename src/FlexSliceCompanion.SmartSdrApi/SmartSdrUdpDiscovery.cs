using FlexSliceCompanion.Core.Radio;
using System.Net;
using System.Net.Sockets;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrUdpDiscovery
{
    private const int DiscoveryPort = 4992;
    private readonly SmartSdrDiscoveryParser _parser = new();

    public async Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        using var client = new UdpClient(AddressFamily.InterNetwork);
        client.ExclusiveAddressUse = false;
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        try
        {
            client.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
        }
        catch (SocketException)
        {
            return [];
        }

        var radios = new Dictionary<string, RadioInfo>(StringComparer.OrdinalIgnoreCase);
        var deadline = DateTimeOffset.UtcNow.AddSeconds(3);

        while (DateTimeOffset.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            var receiveTask = client.ReceiveAsync(cancellationToken).AsTask();
            var delayTask = Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
            var completed = await Task.WhenAny(receiveTask, delayTask);
            if (completed != receiveTask)
            {
                continue;
            }

            var radio = _parser.Parse(receiveTask.Result.Buffer);
            if (radio is not null)
            {
                radios[radio.Serial] = radio;
            }
        }

        return radios.Values.ToArray();
    }
}
