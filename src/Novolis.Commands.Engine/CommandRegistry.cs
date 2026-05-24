using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

/// <summary>Frozen registry of command definitions and known context words.</summary>
public sealed class CommandRegistry(IReadOnlyList<CommandDefinition> definitions) : ICommandRegistry
{
    private readonly FrozenSet<string> _contextWords = definitions
        .Select(d => d.ContextWord)
        .Where(static c => c is not null)
        .Cast<string>()
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyList<CommandDefinition> GetAll() => definitions;

    /// <inheritdoc />
    public bool IsKnownContext(string contextWord) => _contextWords.Contains(contextWord);
}
