namespace FlexSliceCompanion.Plugins.Wsjtx;

public sealed class WsjtxUdpListener
{
    public int Port { get; }

    public WsjtxUdpListener(int port = 2237)
    {
        Port = port;
    }
}
