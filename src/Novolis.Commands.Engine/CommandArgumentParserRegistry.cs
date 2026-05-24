using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

/// <summary>
/// Registry of named argument parsers referenced by <see cref="CommandDefinition.ArgumentParserKey"/>.
/// </summary>
public sealed class CommandArgumentParserRegistry
{
    private readonly Dictionary<string, ICommandArgumentParser> _parsers =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers a parser under a unique key.</summary>
    public CommandArgumentParserRegistry Register(string key, ICommandArgumentParser parser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(parser);
        _parsers[key] = parser;
        return this;
    }

    /// <summary>Looks up a registered parser by key.</summary>
    public bool TryGet(string key, out ICommandArgumentParser? parser) =>
        _parsers.TryGetValue(key, out parser);

    /// <summary>Registered parser keys.</summary>
    public IReadOnlyCollection<string> Keys => _parsers.Keys;

    internal FrozenDictionary<string, ICommandArgumentParser> ToFrozen() =>
        _parsers.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
