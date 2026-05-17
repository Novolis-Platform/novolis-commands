using TUnit.Core;

namespace Novolis.Commands.Abstractions.Tests;

public sealed class ParseResultTests
{
    [Test]
    public async Task Succeeded_Should_Set_Command_And_Success()
    {
        var envelope = CreateEnvelope("test.ok");
        var result = ParseResult.Succeeded(envelope);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command).IsEqualTo(envelope);
        await Assert.That(result.Failures).IsEmpty();
    }

    [Test]
    public async Task Failed_Should_Set_Failures_And_Not_Command()
    {
        var failure = new ParseFailure(ParseFailureCode.UnknownCommand, "nope");
        var result = ParseResult.Failed(failure);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Command).IsNull();
        await Assert.That(result.Failures.Count).IsEqualTo(1);
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.UnknownCommand);
    }

    [Test]
    public async Task Failed_Should_Accept_Multiple_Failures()
    {
        var result = ParseResult.Failed(
            new ParseFailure(ParseFailureCode.MissingArgument, "a"),
            new ParseFailure(ParseFailureCode.InvalidArgument, "b"));

        await Assert.That(result.Failures.Count).IsEqualTo(2);
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
