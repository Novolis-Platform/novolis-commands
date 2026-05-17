namespace Novolis.Commands.Engine;

public sealed class CommandRegistryBuilder
{
    private readonly List<CommandDefinition> _definitions = [];

    public CommandRegistryBuilder Add(
        string name,
        string? context,
        IReadOnlyList<string> verbs,
        params CommandArgumentDefinition[] arguments)
    {
        _definitions.Add(new CommandDefinition(name, context, verbs, arguments));
        return this;
    }

    public ICommandRegistry Build() => new CommandRegistry(_definitions);
}
