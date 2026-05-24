namespace Novolis.Commands;

/// <summary>Well-known system command ids registered by the engine.</summary>
public static class BuiltInCommands
{
    /// <summary>Interrupts the current command (belay).</summary>
    public const string BelayThat = "system.belay-that";

    /// <summary>Clears pending queued commands.</summary>
    public const string ClearQueue = "system.clear-queue";

    /// <summary>Re-queues the last executed command.</summary>
    public const string RepeatLast = "system.repeat-last";

    /// <summary>Lists available commands.</summary>
    public const string Help = "system.help";
}
