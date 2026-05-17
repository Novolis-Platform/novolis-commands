using Novolis.Commands.Queueing;
using TUnit.Core;

namespace Novolis.Commands.Queueing.Tests;

public class ChannelCommandQueueTests
{
    [Test]
    public async Task EnqueueAsync_Should_Preserve_Order()
    {
        var queue = new ChannelCommandQueue();
        var first = CreateEnvelope("first");
        var second = CreateEnvelope("second");

        await queue.EnqueueAsync(first);
        await queue.EnqueueAsync(second);

        using var cts = new CancellationTokenSource();
        var read = queue.ReadAllAsync(cts.Token);
        var enumerator = read.GetAsyncEnumerator(cts.Token);

        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("first");

        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("second");

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
