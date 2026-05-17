namespace Novolis.Commands.Testing;

public sealed class TestCommandContextResolver : ICommandContextResolver<TestCommandContext>
{
    public IReadOnlyDictionary<string, string> GetContextAliases(TestCommandContext context) =>
        context.ContextAliases;

    public IReadOnlyDictionary<string, string> GetVerbAliases(TestCommandContext context) => context.Aliases;
}
