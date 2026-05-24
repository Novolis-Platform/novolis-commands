namespace Novolis.Commands.Testing;

/// <summary>Returns aliases from a <see cref="TestCommandContext"/>.</summary>
public sealed class TestCommandContextResolver : ICommandContextResolver<TestCommandContext>
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> GetContextAliases(TestCommandContext context) =>
        context.ContextAliases;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> GetVerbAliases(TestCommandContext context) => context.Aliases;
}
