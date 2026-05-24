namespace Novolis.Commands.Testing;

/// <summary>Mutable test context for command parsing.</summary>
public sealed class TestCommandContext
{
    /// <summary>Active station/context word, if any.</summary>
    public string? ActiveContextWord { get; init; }

    /// <summary>Maps user tokens to canonical context words.</summary>
    public IReadOnlyDictionary<string, string> ContextAliases { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Maps user verb tokens to expanded phrases.</summary>
    public IReadOnlyDictionary<string, string> Aliases { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
