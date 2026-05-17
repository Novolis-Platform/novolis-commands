using TUnit.Core;

namespace Novolis.Commands.Abstractions.Tests;

public sealed class CommandEnvelopeTests
{
    public static IEnumerable<CommandPriority> Priorities() => Enum.GetValues<CommandPriority>();

    [Test]
    [MethodDataSource(nameof(Priorities))]
    public async Task Envelope_Should_Accept_Priority(CommandPriority priority)
    {
        var envelope = CreateEnvelope(priority: priority);
        await Assert.That(envelope.Priority).IsEqualTo(priority);
    }

    [Test]
    public async Task Default_Envelope_Should_Use_Normal_Priority()
    {
        var envelope = CreateEnvelope();
        await Assert.That(envelope.Priority).IsEqualTo(CommandPriority.Normal);
    }

    [Test]
    public async Task Default_Envelope_Should_Not_Interrupt_Or_Cancel()
    {
        var envelope = CreateEnvelope();
        await Assert.That(envelope.InterruptsCurrentCommand).IsFalse();
        await Assert.That(envelope.CancelsQueuedCommands).IsFalse();
    }

    [Test]
    public async Task CreatedAt_Should_Be_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow;
        var envelope = CreateEnvelope();
        var after = DateTimeOffset.UtcNow;

        await Assert.That(envelope.CreatedAt).IsGreaterThanOrEqualTo(before);
        await Assert.That(envelope.CreatedAt).IsLessThanOrEqualTo(after);
    }

    private static CommandEnvelope CreateEnvelope(CommandPriority priority = CommandPriority.Normal) =>
        new()
        {
            Id = CommandId.New(),
            Name = "test",
            OriginalPrompt = "test",
            ContextWord = "helm",
            Arguments = new Dictionary<string, object?>(),
            Priority = priority
        };
}
