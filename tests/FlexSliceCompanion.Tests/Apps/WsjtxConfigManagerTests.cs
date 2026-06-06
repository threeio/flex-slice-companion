using FlexSliceCompanion.Core.Apps;
using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Slices;
using FlexSliceCompanion.Plugins.Wsjtx;
using Xunit;

namespace FlexSliceCompanion.Tests.Apps;

public sealed class WsjtxConfigManagerTests
{
    [Fact]
    public void BuildLaunchPlan_UsesPerSliceInstanceAndConfigPath()
    {
        var manager = new WsjtxConfigManager();
        var request = new AppLaunchRequest
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
            CatPort = 5101,
            RxAudioEndpoint = Endpoint("DAX RX 1 (FlexRadio DAX)", DaxEndpointType.RxAudio),
            TxAudioEndpoint = Endpoint("DAX TX (FlexRadio DAX)", DaxEndpointType.TxAudio),
            SettingsDirectory = @"C:\Users\Kevin\AppData\Roaming\FlexSliceCompanion\wsjtx\slice-A"
        };

        var plan = manager.BuildLaunchPlan(@"C:\WSJT\bin\wsjtx.exe", request);

        Assert.Equal("wsjtx-slice-A", plan.InstanceId);
        Assert.Contains("--rig-name=\"wsjtx-slice-A\"", plan.Arguments);
        Assert.Contains("--config-path=", plan.Arguments);
    }

    private static DaxEndpoint Endpoint(string name, DaxEndpointType type) =>
        new()
        {
            WindowsDeviceId = name,
            DisplayName = name,
            Generation = DaxGeneration.DaxV2,
            Type = type,
            Channel = type == DaxEndpointType.RxAudio ? 1 : null,
            IsReserved = false
        };
}
