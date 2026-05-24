namespace Novolis.Commands;

/// <summary>Represents CommandEnvelope.</summary>
public sealed record CommandEnvelope
/// <summary>Id.</summary>
{
    /// <summary>OriginalPrompt.</summary>
    public required CommandId Id { get; init; }
    /// <summary>Arguments.</summary>
    public required string Name { get; init; }
    /// <summary>CreatedAt.</summary>
    public required string OriginalPrompt { get; init; }
    /// <summary>CancelsQueuedCommands.</summary>
    public required string? ContextWord { get; init; }
    public required IReadOnlyDictionary<string, object?> Arguments { get; init; }
    public CommandPriority Priority { get; init; } = CommandPriority.Normal;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public bool InterruptsCurrentCommand { get; init; }
    public bool CancelsQueuedCommands { get; init; }
}
