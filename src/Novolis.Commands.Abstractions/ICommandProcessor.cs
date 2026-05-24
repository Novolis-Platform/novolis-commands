namespace Novolis.Commands;

/// <summary>Represents ICommandProcessor<TContext>.</summary>
public interface ICommandProcessor<TContext>
/// <summary>ProcessAsync operation.</summary>
{
    ValueTask ProcessAsync(
        CommandEnvelope command,
        TContext context,
        CancellationToken cancellationToken = default);
}
