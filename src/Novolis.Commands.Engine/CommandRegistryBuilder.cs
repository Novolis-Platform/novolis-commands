namespace Novolis.Commands.Engine;

public sealed class CommandRegistryBuilder
{
    private readonly List<CommandDefinition> _definitions = [];

    public CommandRegistryBuilder Add(
        string name,
        string? context,
        IReadOnlyList<string> verbs,
        params CommandArgumentDefinition[] arguments) =>
        Add(name, context, verbs, argumentParserKey: null, arguments);

    public CommandRegistryBuilder Add(
        string name,
        string? context,
        IReadOnlyList<string> verbs,
        string? argumentParserKey,
        params CommandArgumentDefinition[] arguments)
    {
        _definitions.Add(new CommandDefinition(name, context, verbs, arguments, argumentParserKey));
        return this;
    }

    public ICommandRegistry Build() => Build(validate: true);

    public ICommandRegistry Build(bool validate)
    {
        if (validate)
            CommandRegistryValidator.Validate(_definitions);

        return new CommandRegistry(_definitions);
    }
}
