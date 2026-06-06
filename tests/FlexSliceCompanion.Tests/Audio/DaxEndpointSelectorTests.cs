using FlexSliceCompanion.Core.Audio;
using Xunit;

namespace FlexSliceCompanion.Tests.Audio;

public sealed class DaxEndpointSelectorTests
{
    [Fact]
    public void SelectForSlice_PicksMatchingRxChannelAndTx()
    {
        var detector = new DaxEndpointDetector();
        var scan = detector.Detect([
            new WindowsAudioDevice("rx1", "DAX RX 1 (FlexRadio DAX)"),
            new WindowsAudioDevice("rx2", "DAX RX 2 (FlexRadio DAX)"),
            new WindowsAudioDevice("tx", "DAX TX (FlexRadio DAX)")
        ]);

        var selected = new DaxEndpointSelector().SelectForSlice(scan, 2);

        Assert.Equal(2, selected.Rx.Channel.GetValueOrDefault());
        Assert.Equal(DaxEndpointType.TxAudio, selected.Tx.Type);
    }
}
