namespace Novolis.Commands;

public readonly record struct CommandId(Guid Value)
{
    public static CommandId New() => new(Guid.CreateVersion7());
}
