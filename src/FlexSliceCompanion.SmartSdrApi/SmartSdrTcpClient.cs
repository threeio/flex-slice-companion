namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrTcpClient
{
    public Task ConnectAsync(string host, int port = 4992, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
