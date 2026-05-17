namespace Novolis.Commands.Engine;

public interface ICommandContextResolver<TContext>
{
    string? GetActiveContextWord(TContext context);

    IReadOnlyDictionary<string, string> GetAliases(TContext context);
}
