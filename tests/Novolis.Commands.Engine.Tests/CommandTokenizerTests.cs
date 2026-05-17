using Novolis.Commands.Engine;
using Novolis.Commands.Engine.Tests.Support.Bridge;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandTokenizerTests
{
    public static IEnumerable<(string Prompt, string[] Expected)> BridgeTranscriptTokens() =>
    [
        ("  helm   heading   45  ", ["helm", "heading", "45"]),
        ("conn warp 8", ["conn", "warp", "8"]),
        ("tactical lock target", ["tactical", "lock", "target"]),
        ("nav set course alpha centauri", ["nav", "set", "course", "alpha", "centauri"]),
        ("belay that", ["belay", "that"]),
        ("help tactical", ["help", "tactical"])
    ];

    [Test]
    [MethodDataSource(nameof(BridgeTranscriptTokens))]
    public async Task Tokenize_Should_Match_Bridge_Orders((string Prompt, string[] Expected) data)
    {
        var tokens = CommandTokenizer.Tokenize(data.Prompt.Trim().ToLowerInvariant());
        await Assert.That(tokens).IsEquivalentTo(data.Expected);
    }

    [Test]
    public async Task Tokenize_Kr12Scene_Lines_Should_All_Produce_Tokens()
    {
        var lines = BridgeScenes.Kr12RedAlertEncounter()
            .Orders
            .Where(o => !o.ShouldFail)
            .Select(o => o.Spoken.Trim().ToLowerInvariant());

        foreach (var line in lines)
        {
            var tokens = CommandTokenizer.Tokenize(line);
            await Assert.That(tokens.Length).IsGreaterThan(0);
        }
    }
}
