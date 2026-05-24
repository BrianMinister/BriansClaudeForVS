namespace BriansClaudeVS.Core.SlashCommands;

public class SlashCommandParser : ISlashCommandParser
{
    public ParsedInput Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.StartsWith('/'))
            return new ParsedInput(false, null, input);

        var parts = input.Split(' ', 2, StringSplitOptions.TrimEntries);
        var commandName = parts[0].ToLowerInvariant();
        var argument = parts.Length > 1 ? parts[1] : string.Empty;

        var command = SlashCommands.All.FirstOrDefault(c =>
            c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        return new ParsedInput(true, command, argument);
    }

    public IReadOnlyList<SlashCommandDefinition> GetMatches(string partialInput)
    {
        if (!partialInput.StartsWith('/')) return [];
        var prefix = partialInput.ToLowerInvariant();
        return SlashCommands.All
            .Where(c => c.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
