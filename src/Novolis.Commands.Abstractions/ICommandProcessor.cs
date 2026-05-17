namespace Novolis.Commands;

public interface ICommandProcessor<TContext>
{
    ValueTask ProcessAsync(
        CommandEnvelope command,
        TContext context,
        CancellationToken cancellationToken = default);
}
