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
