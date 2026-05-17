namespace Novolis.Commands.Engine;

public interface ICommandRegistry
{
    IReadOnlyList<CommandDefinition> GetAll();

    bool IsKnownContext(string contextWord);
}
