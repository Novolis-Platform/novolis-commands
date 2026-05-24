namespace Novolis.Commands;

/// <summary>Parsed command ready for queueing or execution.</summary>
public sealed record CommandEnvelope
{
    /// <summary>Stable command identifier.</summary>
    public required CommandId Id { get; init; }

    /// <summary>Registered command name.</summary>
    public required string Name { get; init; }

    /// <summary>Original user prompt text.</summary>
    public required string OriginalPrompt { get; init; }

    /// <summary>Optional context word (e.g. target object).</summary>
    public required string? ContextWord { get; init; }

    /// <summary>Named arguments parsed from the prompt.</summary>
    public required IReadOnlyDictionary<string, object?> Arguments { get; init; }

    /// <summary>Scheduling priority.</summary>
    public CommandPriority Priority { get; init; } = CommandPriority.Normal;

    /// <summary>UTC timestamp when the envelope was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>When true, preempts the currently running command.</summary>
    public bool InterruptsCurrentCommand { get; init; }

    /// <summary>When true, clears pending queued commands before enqueue.</summary>
    public bool CancelsQueuedCommands { get; init; }
}
