namespace Novolis.Commands;

/// <summary>Version-7 GUID identifier for a command instance.</summary>
/// <param name="Value">Underlying GUID value.</param>
public readonly record struct CommandId(Guid Value)
{
    /// <summary>Allocates a new time-ordered command id.</summary>
    public static CommandId New() => new(Guid.CreateVersion7());
}
