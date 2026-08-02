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

    [Test]
    public async Task ClearPendingAsync_Drains_Unread_Commands()
    {
        var queue = new ChannelCommandQueue();
        await queue.EnqueueAsync(CreateEnvelope("first"));
        await queue.EnqueueAsync(CreateEnvelope("second"));

        await queue.ClearPendingAsync();
        await queue.EnqueueAsync(CreateEnvelope("after-clear"));

        using var cts = new CancellationTokenSource();
        var read = queue.ReadAllAsync(cts.Token);
        var enumerator = read.GetAsyncEnumerator(cts.Token);

        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current.Name).IsEqualTo("after-clear");
        cts.Cancel();
    }

    [Test]
    public async Task ClearPendingAsync_Honors_Cancellation()
    {
        var queue = new ChannelCommandQueue();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.That(() => queue.ClearPendingAsync(cts.Token).AsTask())
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
