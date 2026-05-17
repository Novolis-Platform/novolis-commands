namespace Novolis.Commands;

public interface ICommandQueue
{
    ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes commands waiting in the queue that have not started executing.
    /// </summary>
    ValueTask ClearPendingAsync(CancellationToken cancellationToken = default);
}
