namespace Novolis.Commands.Engine;

public sealed record CommandDefinition(
    string Name,
    string? ContextWord,
    IReadOnlyList<string> Verbs,
    IReadOnlyList<CommandArgumentDefinition> Arguments);
