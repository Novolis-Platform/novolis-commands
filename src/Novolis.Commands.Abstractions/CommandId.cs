namespace Novolis.Commands;

/// <summary>Value.</summary>
/// <summary>CommandId operation.</summary>
/// <summary>Represents CommandId.</summary>
public readonly record struct CommandId(Guid Value)
{
    public static CommandId New() => new(Guid.CreateVersion7());
}
