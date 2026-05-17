namespace Novolis.Commands.Engine;

public sealed record CommandArgumentDefinition(
    string Name,
    CommandArgumentKind Kind,
    bool Required = true)
{
    public static CommandArgumentDefinition Integer(string name, bool required = true) =>
        new(name, CommandArgumentKind.Integer, required);

    public static CommandArgumentDefinition String(string name, bool required = true) =>
        new(name, CommandArgumentKind.String, required);
}
