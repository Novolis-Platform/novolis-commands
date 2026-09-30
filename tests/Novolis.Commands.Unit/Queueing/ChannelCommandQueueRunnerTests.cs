using Novolis.Commands.Queueing;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

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
