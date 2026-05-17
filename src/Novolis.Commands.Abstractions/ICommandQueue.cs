namespace Novolis.Commands;

public interface ICommandQueue
{
    ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default);
}
