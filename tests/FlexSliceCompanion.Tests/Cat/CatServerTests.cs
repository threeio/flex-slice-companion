using System.Reactive.Subjects;
using FlexSliceCompanion.Core.Cat;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;
using Xunit;

namespace FlexSliceCompanion.Tests.Cat;

public sealed class CatServerTests
{
    [Fact]
    public async Task HandleCommandAsync_SetFrequencyCallsRadioClient()
    {
        var radio = new FakeRadioClient();
        var server = new CatServer(new CatServerOptions { SliceId = "slice-a", Port = 5101 }, radio);

        var response = await server.HandleCommandAsync("FA00014074000;");

        Assert.Equal("OK;", response);
        Assert.Equal(14074000d, radio.LastFrequencyHz);
    }

    private sealed class FakeRadioClient : IRadioClient
    {
        private readonly Subject<RadioState> _radio = new();
        private readonly Subject<SliceState> _slice = new();

        public double? LastFrequencyHz { get; private set; }

        public IObservable<RadioState> RadioStateChanged => _radio;
        public IObservable<SliceState> SliceChanged => _slice;

        public Task<IReadOnlyList<RadioInfo>> DiscoverAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<RadioInfo>>([]);
        public Task ConnectAsync(RadioInfo radio, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SetSliceFrequencyAsync(string sliceId, double frequencyHz, CancellationToken cancellationToken = default)
        {
            LastFrequencyHz = frequencyHz;
            return Task.CompletedTask;
        }

        public Task SetSliceModeAsync(string sliceId, string mode, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetTxSliceAsync(string sliceId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetPttAsync(string sliceId, bool isTransmit, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
