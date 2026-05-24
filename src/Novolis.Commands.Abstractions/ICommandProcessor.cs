namespace Novolis.Commands;

/// <summary>Executes a parsed <see cref="CommandEnvelope"/> against a context.</summary>
/// <typeparam name="TContext">Execution context type.</typeparam>
public interface ICommandProcessor<TContext>
{
    /// <summary>Runs the command.</summary>
    /// <param name="command">Parsed command envelope.</param>
    /// <param name="context">Execution context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask ProcessAsync(
        CommandEnvelope command,
        TContext context,
        CancellationToken cancellationToken = default);
}
