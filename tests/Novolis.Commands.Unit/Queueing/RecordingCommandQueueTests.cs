using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

public sealed class RecordingCommandQueueTests
{
    public static IEnumerable<int> EnqueueCounts() => Enumerable.Range(1, 25);

    [Test]
    [MethodDataSource(nameof(EnqueueCounts))]
    public async Task Enqueue_Should_Record_All_Commands(int count)
    {
        var queue = new RecordingCommandQueue();

        for (var i = 0; i < count; i++)
            await queue.EnqueueAsync(CreateEnvelope($"cmd-{i}"));

        await Assert.That(queue.Enqueued.Count).IsEqualTo(count);

        for (var i = 0; i < count; i++)
            await Assert.That(queue.Enqueued[i].Name).IsEqualTo($"cmd-{i}");
    }

    [Test]
    public async Task ReadAllAsync_Yields_Enqueued_Commands()
    {
        var queue = new RecordingCommandQueue();
        await queue.EnqueueAsync(CreateEnvelope("alpha"));
        await queue.EnqueueAsync(CreateEnvelope("beta"));

        using var cts = new CancellationTokenSource();
        var read = queue.ReadAllAsync(cts.Token);
        var enumerator = read.GetAsyncEnumerator(cts.Token);

        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("alpha");
        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("beta");
        cts.Cancel();
    }

    [Test]
    public async Task ClearPendingAsync_Drains_Pending_Reads()
    {
        var queue = new RecordingCommandQueue();
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
