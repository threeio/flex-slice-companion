using FlexSliceCompanion.Core.Cat;
using Xunit;

namespace FlexSliceCompanion.Tests.Cat;

public sealed class HrdCatParserTests
{
    private readonly HrdCatParser _parser = new();

    [Fact]
    public void Parse_ReadFrequencyCommand()
    {
        var command = _parser.Parse("FA;");

        Assert.Equal(CatCommandKind.GetFrequency, command.Kind);
    }

    [Fact]
    public void Parse_SetFrequencyCommand()
    {
        var command = _parser.Parse("FA00014074000;");

        Assert.Equal(CatCommandKind.SetFrequency, command.Kind);
        Assert.Equal(14074000d, command.FrequencyHz);
    }

    [Fact]
    public void Parse_SetModeCommand()
    {
        var command = _parser.Parse("MDDIGU;");

        Assert.Equal(CatCommandKind.SetMode, command.Kind);
        Assert.Equal("DIGU", command.Mode);
    }

    [Theory]
    [InlineData("TX1;", true)]
    [InlineData("TX0;", false)]
    public void Parse_SetPttCommand(string raw, bool expected)
    {
        var command = _parser.Parse(raw);

        Assert.Equal(CatCommandKind.SetPtt, command.Kind);
        Assert.Equal(expected, command.Ptt);
    }

    [Theory]
    [InlineData("ID;", CatCommandKind.Identify)]
    [InlineData("AI0;", CatCommandKind.AutoInformation)]
    public void Parse_CompatibilityCommands(string raw, CatCommandKind expected)
    {
        var command = _parser.Parse(raw);

        Assert.Equal(expected, command.Kind);
    }
}
