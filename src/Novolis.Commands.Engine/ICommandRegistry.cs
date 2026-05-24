namespace Novolis.Commands.Engine;

/// <summary>Read-only view of registered command definitions.</summary>
public interface ICommandRegistry
{
    /// <summary>Returns all registered commands.</summary>
    IReadOnlyList<CommandDefinition> GetAll();

    /// <summary>Returns whether <paramref name="contextWord"/> is used by any command.</summary>
    bool IsKnownContext(string contextWord);
}
