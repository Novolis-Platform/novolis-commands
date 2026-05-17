namespace Novolis.Commands.Testing;

public sealed class TestCommandContextResolver : ICommandContextResolver<TestCommandContext>
{
    public string? GetActiveContextWord(TestCommandContext context) => context.ActiveContextWord;

    public IReadOnlyDictionary<string, string> GetAliases(TestCommandContext context) => context.Aliases;
}
