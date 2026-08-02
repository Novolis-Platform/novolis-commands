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
        ("help tactical", ["help", "tactical"]),
        ("helm, set heading to 122", ["helm", "set", "heading", "to", "122"]),
        ("helm, set course 123,5 by 119,4", ["helm", "set", "course", "123,5", "by", "119,4"]),
        ("tactical, lock target!", ["tactical", "lock", "target"])
    ];

    [Test]
    [MethodDataSource(nameof(BridgeTranscriptTokens))]
    public async Task Tokenize_Should_Match_Bridge_Orders((string Prompt, string[] Expected) data)
    {
        var tokens = CommandTokenizer.Tokenize(data.Prompt.Trim().ToLowerInvariant());
        await Assert.That(tokens).IsEquivalentTo(data.Expected);
    }

    [Test]
    public async Task Tokenize_Empty_String_Returns_Empty()
    {
        await Assert.That(CommandTokenizer.Tokenize("")).IsEmpty();
    }

    [Test]
    public async Task Tokenize_Punctuation_Only_Tokens_Returns_Empty()
    {
        await Assert.That(CommandTokenizer.Tokenize(" , ; ")).IsEmpty();
    }
}
