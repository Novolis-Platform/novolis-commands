namespace Novolis.Commands.Engine;

public interface ICommandContextResolver<TContext>
{
    IReadOnlyDictionary<string, string> GetContextAliases(TContext context);

    IReadOnlyDictionary<string, string> GetVerbAliases(TContext context);
}
