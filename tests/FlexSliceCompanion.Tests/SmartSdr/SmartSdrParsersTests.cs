using FlexSliceCompanion.SmartSdrApi;
using Xunit;

namespace FlexSliceCompanion.Tests.SmartSdr;

public sealed class SmartSdrParsersTests
{
    [Fact]
    public void DiscoveryParser_ReadsStatusPayload()
    {
        var radio = new SmartSdrDiscoveryParser().Parse(
            "model=FLEX-6400 serial=1234 version=4.2.18 name=Kevin_Flex callsign=N0CALL ip=192.168.1.50 port=4992");

        Assert.NotNull(radio);
        Assert.Equal("FLEX-6400", radio.Model);
        Assert.Equal("Kevin Flex", radio.Nickname);
        Assert.Equal(new Version(4, 2, 18), radio.SmartSdrVersion);
    }

    [Fact]
    public void SliceStatusParser_ReadsSliceStatus()
    {
        var slice = new SmartSdrSliceStatusParser().Parse(
            "S12345678|slice 0 RF_frequency=14.074000 mode=DIGU dax=1 tx=1 active=1 index_letter=A pan=0x40000000");

        Assert.NotNull(slice);
        Assert.Equal("0", slice.SliceId);
        Assert.Equal("A", slice.Letter);
        Assert.Equal(14074000d, slice.FrequencyHz);
        Assert.Equal("DIGU", slice.Mode);
        Assert.Equal(1, slice.DaxChannel.GetValueOrDefault());
        Assert.True(slice.IsTx);
    }
}
