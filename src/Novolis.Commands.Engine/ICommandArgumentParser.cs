namespace Novolis.Commands.Engine;

/// <summary>
/// Host-provided parser for command argument tokens after verb phrase matching.
/// </summary>
public interface ICommandArgumentParser
{
    /// <summary>Attempts to parse trailing tokens into named arguments.</summary>
    bool TryParse(
        CommandDefinition definition,
        IReadOnlyList<string> argumentTokens,
        out IReadOnlyDictionary<string, object?> arguments,
        out ParseFailure? failure);
}
