namespace Novolis.Commands.Engine;

public interface ICommandContextResolver<TContext>
{
    string? GetActiveContextWord(TContext context);

    IReadOnlyDictionary<string, string> GetContextAliases(TContext context);

    IReadOnlyDictionary<string, string> GetVerbAliases(TContext context);
}
