using Novolis.Commands.Engine;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class BuiltInCommandMatcherTests
{
    private readonly BuiltInCommandMatcher _matcher = new();

    public static IEnumerable<(string Prompt, string ExpectedName)> BuiltInPhrases() =>
    [
        ("belay that", BuiltInCommands.BelayThat),
        ("BELAY THAT", BuiltInCommands.BelayThat),
        ("clear queue", BuiltInCommands.ClearQueue),
        ("repeat last", BuiltInCommands.RepeatLast),
        ("help", BuiltInCommands.Help)
    ];

    public static IEnumerable<string> HelpTopics() =>
    [
        "tactical", "helm", "engineering", "nav", "comms", "weapons", "alpha", "beta", "gamma"
    ];

    public static IEnumerable<string> NonBuiltInPrompts() =>
    [
        "helm heading 90",
        "helpful",
        "helping",
        "belay",
        "clear",
        "repeat",
        "queue",
        "system help",
        ""
    ];

    [Test]
    [MethodDataSource(nameof(BuiltInPhrases))]
    public async Task TryMatch_Should_Match_Known_Phrase((string Prompt, string ExpectedName) data)
    {
        var normalized = data.Prompt.ToLowerInvariant();
        var matched = _matcher.TryMatch(normalized, out var envelope);

        await Assert.That(matched).IsTrue();
        await Assert.That(envelope!.Name).IsEqualTo(data.ExpectedName);
        await Assert.That(envelope.OriginalPrompt).IsEqualTo(normalized);
    }

    [Test]
    [MethodDataSource(nameof(HelpTopics))]
    public async Task TryMatch_Should_Parse_Help_Topic(string topic)
    {
        var prompt = $"help {topic}";
        var matched = _matcher.TryMatch(prompt, out var envelope);

        await Assert.That(matched).IsTrue();
        await Assert.That(envelope!.Name).IsEqualTo(BuiltInCommands.Help);
        await Assert.That(envelope.Arguments["topic"]).IsEqualTo(topic);
    }

    [Test]
    [MethodDataSource(nameof(NonBuiltInPrompts))]
    public async Task TryMatch_Should_Not_Match_NonBuiltIn(string prompt)
    {
        var matched = _matcher.TryMatch(prompt.ToLowerInvariant(), out _);
        await Assert.That(matched).IsFalse();
    }

    [Test]
    public async Task TryMatch_BelayThat_Should_Set_Emergency_Interrupt()
    {
        _matcher.TryMatch("belay that", out var envelope);

        await Assert.That(envelope!.Priority).IsEqualTo(CommandPriority.Emergency);
        await Assert.That(envelope.InterruptsCurrentCommand).IsTrue();
    }

    [Test]
    public async Task TryMatch_ClearQueue_Should_Cancel_Queued()
    {
        _matcher.TryMatch("clear queue", out var envelope);

        await Assert.That(envelope!.Priority).IsEqualTo(CommandPriority.High);
        await Assert.That(envelope.CancelsQueuedCommands).IsTrue();
    }
}
