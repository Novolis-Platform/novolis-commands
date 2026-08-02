using Novolis.Commands.Queueing;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

public sealed class ManualCommandQueueTests
{
    [Test]
    public async Task ReadAllAsync_Yields_Enqueued_Commands()
    {
        var queue = new ManualCommandQueue();
        using var cts = new CancellationTokenSource();

        var readTask = Task.Run(async () =>
        {
            var names = new List<string>();
            try
            {
                await foreach (var cmd in queue.ReadAllAsync(cts.Token))
                    names.Add(cmd.Name);
            }
            catch (OperationCanceledException)
            {
            }

            return names;
        });

        await queue.EnqueueAsync(CreateEnvelope("one"));
        await queue.EnqueueAsync(CreateEnvelope("two"));
        await Task.Delay(100);
        cts.Cancel();

        var names = await readTask.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(names).IsEquivalentTo(["one", "two"]);
    }

    [Test]
    public async Task ClearPendingAsync_Removes_Unread_Commands()
    {
        var queue = new ManualCommandQueue();
        await queue.EnqueueAsync(CreateEnvelope("first"));
        await queue.EnqueueAsync(CreateEnvelope("second"));

        await queue.ClearPendingAsync();
        await queue.EnqueueAsync(CreateEnvelope("after-clear"));

        using var cts = new CancellationTokenSource();
        var read = queue.ReadAllAsync(cts.Token);
        var enumerator = read.GetAsyncEnumerator(cts.Token);

        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("after-clear");
    }

    [Test]
    public async Task EnqueueAsync_Honors_Cancellation()
    {
        var queue = new ManualCommandQueue();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.That(() => queue.EnqueueAsync(CreateEnvelope("x"), cts.Token).AsTask())
            .Throws<OperationCanceledException>();
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
}

public sealed class ChannelCommandQueueRunnerTests
{
    [Test]
    public async Task RunAsync_Processes_Commands_From_ChannelQueue()
    {
        var queue = new ChannelCommandQueue();
        var processor = new RecordingProcessor();
        var runner = new CommandQueueRunner<object>(queue, processor);

        using var runCts = new CancellationTokenSource();
        var runTask = runner.RunAsync(new object(), runCts.Token);

        await queue.EnqueueAsync(CreateEnvelope("alpha"));
        await queue.EnqueueAsync(CreateEnvelope("beta"));

        await processor.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(processor.Names).IsEquivalentTo(["alpha", "beta"]);

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

    private sealed class RecordingProcessor : ICommandProcessor<object>
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
