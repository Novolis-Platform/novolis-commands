namespace Novolis.Commands.Queueing;

public sealed class CommandQueueRunner<TContext>(
    ICommandQueue queue,
    ICommandProcessor<TContext> processor)
{
    private CancellationTokenSource? _currentCommandCts;

    public async Task RunAsync(TContext context, CancellationToken cancellationToken)
    {
        Task? inFlight = null;

        await foreach (var command in queue.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (command.CancelsQueuedCommands)
                await queue.ClearPendingAsync(cancellationToken).ConfigureAwait(false);

            if (inFlight is not null)
            {
                if (command.InterruptsCurrentCommand)
                    await CancelCurrentAsync().ConfigureAwait(false);

                await inFlight.ConfigureAwait(false);
                inFlight = null;
            }

            _currentCommandCts?.Dispose();
            _currentCommandCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            inFlight = processor.ProcessAsync(command, context, _currentCommandCts.Token).AsTask();
        }

        if (inFlight is not null)
            await inFlight.ConfigureAwait(false);

        _currentCommandCts?.Dispose();
        _currentCommandCts = null;
    }

    private async ValueTask CancelCurrentAsync()
    {
        _currentCommandCts?.Cancel();
        await ValueTask.CompletedTask;
    }
}
