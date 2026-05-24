namespace Novolis.Commands.Engine;

/// <summary>Supplies context and verb aliases for parsing.</summary>
/// <typeparam name="TContext">Execution context type.</typeparam>
public interface ICommandContextResolver<TContext>
{
    /// <summary>Maps user context tokens to canonical context words.</summary>
    IReadOnlyDictionary<string, string> GetContextAliases(TContext context);

    /// <summary>Maps user verb tokens to expanded verb phrases.</summary>
    IReadOnlyDictionary<string, string> GetVerbAliases(TContext context);
}
