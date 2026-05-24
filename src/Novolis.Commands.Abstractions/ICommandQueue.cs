namespace Novolis.Commands;

/// <summary>Thread-safe queue of <see cref="CommandEnvelope"/> instances for sequential execution.</summary>
public interface ICommandQueue
{
    /// <summary>Adds a command to the queue.</summary>
    /// <param name="command">Command to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default);

    /// <summary>Reads commands as they become available.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Removes commands waiting in the queue that have not started executing.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask ClearPendingAsync(CancellationToken cancellationToken = default);
}
