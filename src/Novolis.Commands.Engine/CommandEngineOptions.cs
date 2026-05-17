namespace Novolis.Commands.Engine;

/// <summary>
/// Optional configuration for <see cref="CommandEngine{TContext}"/>.
/// </summary>
public sealed class CommandEngineOptions
{
    public CommandArgumentParserRegistry ArgumentParsers { get; } = new();

    public BuiltInCommandMatcher? BuiltInMatcher { get; set; }
}
