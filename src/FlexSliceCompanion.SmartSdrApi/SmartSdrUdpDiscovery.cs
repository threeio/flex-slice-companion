using FlexSliceCompanion.Core.Radio;

namespace FlexSliceCompanion.SmartSdrApi;

public sealed class SmartSdrUdpDiscovery
{
    public Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<RadioInfo>>([]);
    }
}
