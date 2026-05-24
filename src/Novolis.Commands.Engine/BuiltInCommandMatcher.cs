using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

/// <summary>Matches system built-in phrases (help, belay that, etc.).</summary>
public sealed class BuiltInCommandMatcher
{
    private static readonly FrozenDictionary<string, Func<string, CommandEnvelope>> Phrases =
        new Dictionary<string, Func<string, CommandEnvelope>>(StringComparer.OrdinalIgnoreCase)
        {
            ["belay that"] = CreateBuiltIn(BuiltInCommands.BelayThat, CommandPriority.Emergency, interrupt: true),
            ["clear queue"] = CreateBuiltIn(BuiltInCommands.ClearQueue, CommandPriority.High, cancelQueue: true),
            ["repeat last"] = CreateBuiltIn(BuiltInCommands.RepeatLast, CommandPriority.Normal),
            ["help"] = CreateHelp(null)
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Attempts to build a built-in envelope for a normalized prompt.</summary>
    public bool TryMatch(string normalizedPrompt, out CommandEnvelope? envelope)
    {
        if (Phrases.TryGetValue(normalizedPrompt, out var factory))
        {
            envelope = factory(normalizedPrompt);
            return true;
        }

        if (normalizedPrompt.AsSpan().StartsWith("help ", StringComparison.Ordinal))
        {
            var topic = normalizedPrompt.AsSpan(5..).Trim().ToString();
            envelope = CreateHelp(string.IsNullOrEmpty(topic) ? null : topic)(normalizedPrompt);
            return true;
        }

        envelope = null;
        return false;
    }

    private static Func<string, CommandEnvelope> CreateBuiltIn(
        string name,
        CommandPriority priority,
        bool interrupt = false,
        bool cancelQueue = false) =>
        prompt => new CommandEnvelope
        {
            Id = CommandId.New(),
            Name = name,
            OriginalPrompt = prompt,
            ContextWord = null,
            Arguments = new Dictionary<string, object?>(),
            Priority = priority,
            InterruptsCurrentCommand = interrupt,
            CancelsQueuedCommands = cancelQueue
        };

    private static Func<string, CommandEnvelope> CreateHelp(string? topic) =>
        prompt => new CommandEnvelope
        {
            Id = CommandId.New(),
            Name = BuiltInCommands.Help,
            OriginalPrompt = prompt,
            ContextWord = null,
            Arguments = new Dictionary<string, object?> { ["topic"] = topic },
            Priority = CommandPriority.Normal
        };
}
