namespace BriansClaudeVS.Core.SlashCommands;

public record SlashCommandDefinition(
    string Name,
    string Description,
    string SystemPrompt,
    bool RequiresSelection);

public static class SlashCommands
{
    public static IReadOnlyList<SlashCommandDefinition> All { get; } =
    [
        new("/explain",
            "Explain selected code",
            "You are a senior software engineer. Explain the provided code clearly and concisely, " +
            "covering what it does, how it works, and any notable patterns or potential issues.",
            RequiresSelection: true),

        new("/fix",
            "Fix errors or issues in selection",
            "You are a senior software engineer. Identify and fix bugs, errors, or issues in the " +
            "provided code. Explain what was wrong and show the corrected version.",
            RequiresSelection: true),

        new("/tests",
            "Generate unit tests",
            "You are a senior software engineer. Generate comprehensive unit tests for the provided " +
            "code. Use the testing framework already in the project. Cover happy paths, edge cases, " +
            "and error conditions.",
            RequiresSelection: true),

        new("/doc",
            "Add XML documentation comments",
            "You are a senior software engineer. Add complete XML documentation comments to the " +
            "provided C# code. Include <summary>, <param>, <returns>, and <exception> tags as " +
            "appropriate. Return only the documented code.",
            RequiresSelection: true),

        new("/refactor",
            "Suggest refactoring improvements",
            "You are a senior software engineer. Suggest and apply refactoring improvements to the " +
            "provided code. Focus on readability, maintainability, and adherence to SOLID principles. " +
            "Explain your changes.",
            RequiresSelection: true),
    ];
}
