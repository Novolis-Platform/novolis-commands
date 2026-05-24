using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

public sealed class ManualCommandQueueTests
{
    public static IEnumerable<int> QueueDepths() => Enumerable.Range(1, 30);

    [Test]
    [MethodDataSource(nameof(QueueDepths))]
    public async Task ReadAllAsync_Should_Preserve_Enqueue_Order(int depth)
    {
        var queue = new ManualCommandQueue();
        var expected = Enumerable.Range(0, depth)
            .Select(i => CreateEnvelope($"order-{i}"))
            .ToArray();

        foreach (var envelope in expected)
            await queue.EnqueueAsync(envelope);

        using var cts = new CancellationTokenSource();
        var read = queue.ReadAllAsync(cts.Token);
        var enumerator = read.GetAsyncEnumerator(cts.Token);

        for (var i = 0; i < depth; i++)
        {
            await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
            await Assert.That(enumerator.Current.Name).IsEqualTo(expected[i].Name);
        }

        cts.Cancel();
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
