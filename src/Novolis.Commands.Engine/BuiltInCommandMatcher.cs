namespace Novolis.Commands.Engine;

public sealed class BuiltInCommandMatcher
{
    private static readonly Dictionary<string, Func<string, CommandEnvelope>> Phrases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["belay that"] = prompt => new CommandEnvelope
            {
                Id = CommandId.New(),
                Name = BuiltInCommands.BelayThat,
                OriginalPrompt = prompt,
                ContextWord = null,
                Arguments = new Dictionary<string, object?>(),
                Priority = CommandPriority.Emergency,
                InterruptsCurrentCommand = true,
                CancelsQueuedCommands = false
            },
            ["clear queue"] = prompt => new CommandEnvelope
            {
                Id = CommandId.New(),
                Name = BuiltInCommands.ClearQueue,
                OriginalPrompt = prompt,
                ContextWord = null,
                Arguments = new Dictionary<string, object?>(),
                Priority = CommandPriority.High,
                InterruptsCurrentCommand = false,
                CancelsQueuedCommands = true
            },
            ["repeat last"] = prompt => new CommandEnvelope
            {
                Id = CommandId.New(),
                Name = BuiltInCommands.RepeatLast,
                OriginalPrompt = prompt,
                ContextWord = null,
                Arguments = new Dictionary<string, object?>(),
                Priority = CommandPriority.Normal,
                InterruptsCurrentCommand = false,
                CancelsQueuedCommands = false
            }
        };

    public bool TryMatch(string normalizedPrompt, out CommandEnvelope? envelope)
    {
        if (Phrases.TryGetValue(normalizedPrompt, out var factory))
        {
            envelope = factory(normalizedPrompt);
            return true;
        }

        envelope = null;
        return false;
    }
}
