using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

public sealed class CommandRegistry(IReadOnlyList<CommandDefinition> definitions) : ICommandRegistry
{
    private readonly FrozenSet<string> _contextWords = definitions
        .Select(d => d.ContextWord)
        .Where(static c => c is not null)
        .Cast<string>()
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<CommandDefinition> GetAll() => definitions;

    public bool IsKnownContext(string contextWord) => _contextWords.Contains(contextWord);
}
