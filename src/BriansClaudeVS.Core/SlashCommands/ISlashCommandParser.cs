namespace BriansClaudeVS.Core.SlashCommands;

public record ParsedInput(
    bool IsSlashCommand,
    SlashCommandDefinition? Command,
    string Argument);

public interface ISlashCommandParser
{
    ParsedInput Parse(string input);
    IReadOnlyList<SlashCommandDefinition> GetMatches(string partialInput);
}
