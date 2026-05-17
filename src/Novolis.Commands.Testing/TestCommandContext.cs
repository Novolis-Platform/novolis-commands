namespace Novolis.Commands.Testing;

public sealed class TestCommandContext
{
    public string? ActiveContextWord { get; init; }

    public IReadOnlyDictionary<string, string> ContextAliases { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Aliases { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
