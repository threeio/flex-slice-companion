using FlexSliceCompanion.Core.Audio;
using Xunit;

namespace FlexSliceCompanion.Tests.Audio;

public sealed class DaxEndpointDetectorTests
{
    private readonly DaxEndpointDetector _detector = new();

    [Theory]
    [InlineData("DAX Audio RX 1", DaxGeneration.DaxV1, DaxEndpointType.RxAudio, 1, false)]
    [InlineData("DAX Audio TX", DaxGeneration.DaxV1, DaxEndpointType.TxAudio, null, false)]
    [InlineData("DAX Mic Audio", DaxGeneration.DaxV1, DaxEndpointType.MicAudio, null, false)]
    [InlineData("DAX IQ RX 4", DaxGeneration.DaxV1, DaxEndpointType.IqAudio, 4, false)]
    [InlineData("DAX RESERVED AUDIO RX 1", DaxGeneration.DaxV1, DaxEndpointType.Reserved, 1, true)]
    [InlineData("DAX RESERVED AUDIO TX", DaxGeneration.DaxV1, DaxEndpointType.Reserved, null, true)]
    [InlineData("DAX RESERVED MIC AUDIO", DaxGeneration.DaxV1, DaxEndpointType.Reserved, null, true)]
    [InlineData("DAX RESERVED IQ RX 1", DaxGeneration.DaxV1, DaxEndpointType.Reserved, 1, true)]
    [InlineData("DAX RX 1 (FlexRadio DAX)", DaxGeneration.DaxV2, DaxEndpointType.RxAudio, 1, false)]
    [InlineData("DAX TX (FlexRadio DAX)", DaxGeneration.DaxV2, DaxEndpointType.TxAudio, null, false)]
    [InlineData("DAX Mic (FlexRadio DAX)", DaxGeneration.DaxV2, DaxEndpointType.MicAudio, null, false)]
    [InlineData("DAX IQ 3 (FlexRadio DAX)", DaxGeneration.DaxV2, DaxEndpointType.IqAudio, 3, false)]
    public void Parse_NormalizesKnownDaxEndpointNames(
        string displayName,
        DaxGeneration generation,
        DaxEndpointType type,
        int? channel,
        bool reserved)
    {
        var endpoint = _detector.Parse(new WindowsAudioDevice("id", displayName));

        Assert.Equal(generation, endpoint.Generation);
        Assert.Equal(type, endpoint.Type);
        Assert.Equal(channel, endpoint.Channel);
        Assert.Equal(reserved, endpoint.IsReserved);
    }

    [Fact]
    public void Detect_WarnsWhenBothDaxGenerationsArePresent()
    {
        var scan = _detector.Detect([
            new WindowsAudioDevice("one", "DAX Audio RX 1"),
            new WindowsAudioDevice("two", "DAX RX 1 (FlexRadio DAX)")
        ]);

        Assert.True(scan.HasDaxV1);
        Assert.True(scan.HasDaxV2);
        Assert.Equal(DaxGeneration.DaxV2, scan.PreferredGeneration);
        Assert.NotNull(scan.Warning);
    }

    [Fact]
    public void GetAssignableEndpoints_FiltersReservedDevices()
    {
        var scan = _detector.Detect([
            new WindowsAudioDevice("reserved", "DAX RESERVED AUDIO RX 1"),
            new WindowsAudioDevice("rx", "DAX Audio RX 1"),
            new WindowsAudioDevice("tx", "DAX Audio TX")
        ]);

        var assignable = _detector.GetAssignableEndpoints(scan);

        Assert.DoesNotContain(assignable, endpoint => endpoint.IsReserved);
        Assert.Equal(2, assignable.Count);
    }

    [Fact]
    public void Detect_ExplainsDaxV2VisibilityWhenNoDevicesAreFound()
    {
        var scan = _detector.Detect([]);

        Assert.Equal(DaxGeneration.Unknown, scan.PreferredGeneration);
        Assert.NotNull(scan.Message);
    }
}
