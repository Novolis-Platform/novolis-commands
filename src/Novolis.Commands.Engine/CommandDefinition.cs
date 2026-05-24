namespace Novolis.Commands.Engine;

/// <summary>Registered command with verbs, context, and argument schema.</summary>
/// <param name="Name">Stable command name.</param>
/// <param name="ContextWord">Required station/context prefix, or null for global commands.</param>
/// <param name="Verbs">Phrase tokens that invoke this command.</param>
/// <param name="Arguments">Positional argument definitions.</param>
/// <param name="ArgumentParserKey">Optional named parser from <see cref="CommandEngineOptions.ArgumentParsers"/>.</param>
public sealed record CommandDefinition(
    string Name,
    string? ContextWord,
    IReadOnlyList<string> Verbs,
    IReadOnlyList<CommandArgumentDefinition> Arguments,
    string? ArgumentParserKey = null);
