namespace Novolis.Commands.Engine;

/// <summary>
/// Optional configuration for <see cref="CommandEngine{TContext}"/>.
/// </summary>
public sealed class CommandEngineOptions
{
    /// <summary>Registry of named argument parsers referenced by <see cref="CommandDefinition.ArgumentParserKey"/>.</summary>
    public CommandArgumentParserRegistry ArgumentParsers { get; } = new();

    /// <summary>Optional matcher for system built-in commands.</summary>
    public BuiltInCommandMatcher? BuiltInMatcher { get; set; }
}
