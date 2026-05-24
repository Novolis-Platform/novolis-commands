namespace Novolis.Commands.Engine;

/// <summary>Fluent builder for <see cref="ICommandRegistry"/>.</summary>
public sealed class CommandRegistryBuilder
{
    private readonly List<CommandDefinition> _definitions = [];

    /// <summary>Adds a command without a custom argument parser key.</summary>
    public CommandRegistryBuilder Add(
        string name,
        string? context,
        IReadOnlyList<string> verbs,
        params CommandArgumentDefinition[] arguments) =>
        Add(name, context, verbs, argumentParserKey: null, arguments);

    /// <summary>Adds a command with an optional named argument parser.</summary>
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

    /// <summary>Builds a validated registry.</summary>
    public ICommandRegistry Build() => Build(validate: true);

    /// <summary>Builds a registry, optionally skipping validation (for tests).</summary>
    public ICommandRegistry Build(bool validate)
    {
        if (validate)
            CommandRegistryValidator.Validate(_definitions);

        return new CommandRegistry(_definitions);
    }
}
