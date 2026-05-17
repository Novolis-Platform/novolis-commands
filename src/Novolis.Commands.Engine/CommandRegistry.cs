namespace Novolis.Commands.Engine;

public sealed class CommandRegistry(IReadOnlyList<CommandDefinition> definitions) : ICommandRegistry
{
    public IReadOnlyList<CommandDefinition> GetAll() => definitions;
}
