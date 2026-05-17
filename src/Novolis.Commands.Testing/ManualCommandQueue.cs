namespace Novolis.Commands.Testing;

/// <summary>
/// Deterministic queue for tests without channel reader timing dependencies.
/// </summary>
public sealed class ManualCommandQueue : ICommandQueue
{
    private readonly Queue<CommandEnvelope> _pending = new();
    private readonly Lock _lock = new();
    private TaskCompletionSource? _signal;

    public async ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource? toRelease;

        lock (_lock)
        {
            _pending.Enqueue(command);
            toRelease = _signal;
            _signal = null;
        }

        toRelease?.TrySetResult();
        await ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CommandEnvelope? next = null;
            lock (_lock)
            {
                if (_pending.Count > 0)
                    next = _pending.Dequeue();
                else
                    _signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            if (next is not null)
            {
                yield return next;
                continue;
            }

            var signal = _signal!;
            await signal.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
