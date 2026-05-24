using BriansClaudeVS.Core.SlashCommands;
using Xunit;

namespace BriansClaudeVS.Tests;

public class SlashCommandParserTests
{
    private readonly SlashCommandParser _parser = new();

    [Fact]
    public void Parse_PlainText_IsNotSlashCommand()
    {
        var result = _parser.Parse("hello world");
        Assert.False(result.IsSlashCommand);
        Assert.Null(result.Command);
        Assert.Equal("hello world", result.Argument);
    }

    [Fact]
    public void Parse_ExplainCommand_ReturnsCorrectDefinition()
    {
        var result = _parser.Parse("/explain this code here");
        Assert.True(result.IsSlashCommand);
        Assert.NotNull(result.Command);
        Assert.Equal("/explain", result.Command.Name);
        Assert.Equal("this code here", result.Argument);
    }

    [Fact]
    public void Parse_UnknownSlashCommand_HasNullDefinition()
    {
        var result = _parser.Parse("/unknown stuff");
        Assert.True(result.IsSlashCommand);
        Assert.Null(result.Command);
    }

    [Theory]
    [InlineData("/explain")]
    [InlineData("/fix")]
    [InlineData("/tests")]
    [InlineData("/doc")]
    [InlineData("/refactor")]
    public void Parse_AllKnownCommands_ResolveProperly(string command)
    {
        var result = _parser.Parse(command);
        Assert.True(result.IsSlashCommand);
        Assert.NotNull(result.Command);
        Assert.Equal(command.ToLower(), result.Command.Name);
    }

    [Fact]
    public void GetMatches_PartialInput_ReturnsFilteredList()
    {
        var matches = _parser.GetMatches("/f");
        Assert.Contains(matches, m => m.Name == "/fix");
        Assert.DoesNotContain(matches, m => m.Name == "/explain");
    }

    [Fact]
    public void GetMatches_NonSlashInput_ReturnsEmpty()
    {
        var matches = _parser.GetMatches("hello");
        Assert.Empty(matches);
    }
}
