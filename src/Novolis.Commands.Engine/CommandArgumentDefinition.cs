namespace Novolis.Commands.Engine;

/// <summary>Schema for a single positional command argument.</summary>
public sealed record CommandArgumentDefinition(
    string Name,
    CommandArgumentKind Kind,
    bool Required = true)
{
    /// <summary>Creates an integer argument definition.</summary>
    public static CommandArgumentDefinition Integer(string name, bool required = true) =>
        new(name, CommandArgumentKind.Integer, required);

    /// <summary>Creates a string argument definition.</summary>
    public static CommandArgumentDefinition String(string name, bool required = true) =>
        new(name, CommandArgumentKind.String, required);

    /// <summary>Creates a double argument definition.</summary>
    public static CommandArgumentDefinition Double(string name, bool required = true) =>
        new(name, CommandArgumentKind.Double, required);
}
