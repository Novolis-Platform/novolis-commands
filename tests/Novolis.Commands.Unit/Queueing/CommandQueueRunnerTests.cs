using Novolis.Commands.Queueing;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

public class CommandQueueRunnerTests
{
    [Test]
    public async Task RunAsync_BelayThat_Should_Cancel_InFlight_Command()
    {
        var queue = new ManualCommandQueue();
        var processor = new CancellingProcessor();
        var runner = new CommandQueueRunner<object>(queue, processor);

        using var runCts = new CancellationTokenSource();
        var runTask = runner.RunAsync(new object(), runCts.Token);

        await queue.EnqueueAsync(CreateEnvelope("long.running"));
        await processor.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await queue.EnqueueAsync(new CommandEnvelope
        {
            Id = CommandId.New(),
            Name = BuiltInCommands.BelayThat,
            OriginalPrompt = "belay that",
            ContextWord = null,
            Arguments = new Dictionary<string, object?>(),
            Priority = CommandPriority.Emergency,
            InterruptsCurrentCommand = true
        });

        await processor.BelayObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(processor.LongRunningCancelled).IsTrue();

        runCts.Cancel();
        try
        {
            await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Test]
    public async Task RunAsync_ClearQueue_Should_Drain_Pending_Commands()
    {
        var queue = new ManualCommandQueue();
        var processor = new ClearQueueRecordingProcessor();
        var runner = new CommandQueueRunner<object>(queue, processor);

        using var runCts = new CancellationTokenSource();
        var runTask = runner.RunAsync(new object(), runCts.Token);

        await queue.EnqueueAsync(CreateEnvelope("first"));
        await queue.EnqueueAsync(CreateEnvelope("second"));
        await queue.EnqueueAsync(new CommandEnvelope
        {
            Id = CommandId.New(),
            Name = BuiltInCommands.ClearQueue,
            OriginalPrompt = "clear queue",
            ContextWord = null,
            Arguments = new Dictionary<string, object?>(),
            CancelsQueuedCommands = true
        });
        await queue.EnqueueAsync(CreateEnvelope("third"));

        await processor.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(processor.Names).IsEquivalentTo(
            ["first", "second", BuiltInCommands.ClearQueue]);

        runCts.Cancel();
        try
        {
            await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Test]
    public async Task RunAsync_Processes_NonInterrupting_Sequence()
    {
        var queue = new ManualCommandQueue();
        var processor = new SequentialRecordingProcessor();
        var runner = new CommandQueueRunner<object>(queue, processor);

        using var runCts = new CancellationTokenSource();
        var runTask = runner.RunAsync(new object(), runCts.Token);

        await queue.EnqueueAsync(CreateEnvelope("first"));
        await queue.EnqueueAsync(CreateEnvelope("second"));
        await processor.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(processor.Names).IsEquivalentTo(["first", "second"]);

        runCts.Cancel();
        try
        {
            await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static CommandEnvelope CreateEnvelope(string name) =>
        new()
        {
            Id = CommandId.New(),
            Name = name,
            OriginalPrompt = name,
            ContextWord = null,
            Arguments = new Dictionary<string, object?>()
        };

    private sealed class CancellingProcessor : ICommandProcessor<object>
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource BelayObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool LongRunningCancelled { get; private set; }

        public async ValueTask ProcessAsync(
            CommandEnvelope command,
            object context,
            CancellationToken cancellationToken)
        {
            if (command.Name == BuiltInCommands.BelayThat)
            {
                BelayObserved.TrySetResult();
                return;
            }

            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                LongRunningCancelled = true;
            }
        }
    }

    private sealed class ClearQueueRecordingProcessor : ICommandProcessor<object>
    {
        public List<string> Names { get; } = [];
        public TaskCompletionSource Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask ProcessAsync(
            CommandEnvelope command,
            object context,
            CancellationToken cancellationToken)
        {
            Names.Add(command.Name);
            if (command.Name == BuiltInCommands.ClearQueue)
                Processed.TrySetResult();

            return ValueTask.CompletedTask;
        }
    }

    private sealed class SequentialRecordingProcessor : ICommandProcessor<object>
    {
        public List<string> Names { get; } = [];
        public TaskCompletionSource Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask ProcessAsync(
            CommandEnvelope command,
            object context,
            CancellationToken cancellationToken)
        {
            Names.Add(command.Name);
            if (Names.Count == 2)
                Processed.TrySetResult();

            return ValueTask.CompletedTask;
        }
    }
}
