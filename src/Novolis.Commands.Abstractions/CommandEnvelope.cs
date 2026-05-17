namespace Novolis.Commands;

public sealed record CommandEnvelope
{
    public required CommandId Id { get; init; }
    public required string Name { get; init; }
    public required string OriginalPrompt { get; init; }
    public required string? ContextWord { get; init; }
    public required IReadOnlyDictionary<string, object?> Arguments { get; init; }
    public CommandPriority Priority { get; init; } = CommandPriority.Normal;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public bool InterruptsCurrentCommand { get; init; }
    public bool CancelsQueuedCommands { get; init; }
}
