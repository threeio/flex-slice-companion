using FlexSliceCompanion.Core.Apps;
using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;
using Xunit;

namespace FlexSliceCompanion.Tests.Apps;

public sealed class SliceSessionManagerTests
{
    [Fact]
    public async Task StartAsync_SelectsDaxStartsAppAndAssignsCatPort()
    {
        using var radio = new DemoRadioClient();
        var app = new FakeExternalApp();
        var resolver = new FakeResolver();
        var manager = new SliceSessionManager(radio, app, resolver);
        var scan = new DaxEndpointDetector().Detect([
            new WindowsAudioDevice("rx", "DAX RX 1 (FlexRadio DAX)"),
            new WindowsAudioDevice("tx", "DAX TX (FlexRadio DAX)")
        ]);

        var session = await manager.StartAsync(new SliceSessionRequest
        {
            Slice = new SliceState
            {
                SliceId = "0",
                Letter = "A",
                FrequencyHz = 14074000,
                Mode = "DIGU",
                DaxChannel = 1,
                IsTx = true,
                IsActive = true
            },
            DaxScanResult = scan,
            SettingsRoot = "root",
            BaseCatPort = 5100
        });

        Assert.Equal(5101, session.CatPort);
        Assert.Equal("fake-wsjtx-slice-A", session.SessionId);
        Assert.True(app.Launched);

        await manager.DisposeAsync();
    }

    private sealed class FakeExternalApp : IExternalRadioApp
    {
        public bool Launched { get; private set; }
        public string Name => "Fake WSJT-X";
        public bool IsInstalled() => true;

        public Task<AppLaunchResult> LaunchAsync(AppLaunchRequest request, CancellationToken cancellationToken = default)
        {
            Launched = true;
            return Task.FromResult(new AppLaunchResult
            {
                InstanceId = $"fake-wsjtx-slice-{request.Slice.Letter}",
                ExecutablePath = "fake.exe"
            });
        }

        public Task StopAsync(string instanceId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeResolver : IAppSettingsDirectoryResolver
    {
        public string ResolveSettingsDirectory(string root, string sliceLetter) => Path.Combine(root, sliceLetter);
    }
}
