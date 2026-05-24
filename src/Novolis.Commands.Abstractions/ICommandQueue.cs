namespace Novolis.Commands;

/// <summary>Represents ICommandQueue.</summary>
public interface ICommandQueue
/// <summary>EnqueueAsync operation.</summary>
{
    ValueTask EnqueueAsync(
        CommandEnvelope command,
        /// <summary>ReadAllAsync operation.</summary>
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes commands waiting in the queue that have not started executing.
    /// </summary>
    ValueTask ClearPendingAsync(CancellationToken cancellationToken = default);
}
