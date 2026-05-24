namespace Novolis.Commands;

/// <summary>Relative priority for queued commands.</summary>
public enum CommandPriority
{
    /// <summary>Runs after normal and high priority work.</summary>
    Low,

    /// <summary>Default scheduling priority.</summary>
    Normal,

    /// <summary>Runs before low and normal priority work.</summary>
    High,

    /// <summary>Highest priority; may preempt other commands.</summary>
    Emergency
}
