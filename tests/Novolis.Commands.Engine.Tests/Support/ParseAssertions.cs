using Novolis.Commands.Engine;
using Novolis.Commands.Testing;
using TUnit.Assertions;

namespace Novolis.Commands.Engine.Tests.Support;

internal static class ParseAssertions
{
    public static async Task AssertEngineCase(
        CommandEngine<TestCommandContext> engine,
        TestCommandContext context,
        ParseCase testCase)
    {
        var result = await engine.ParseCommandAsync(testCase.Prompt, context);

        if (testCase.ExpectSuccess)
        {
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Command).IsNotNull();
            await Assert.That(result.Command!.Name).IsEqualTo(testCase.ExpectedCommandName!);

            if (testCase.ExpectedContext is not null)
                await Assert.That(result.Command.ContextWord).IsEqualTo(testCase.ExpectedContext);

            if (testCase.ExpectedArguments is not null)
            {
                foreach (var (key, expected) in testCase.ExpectedArguments)
                    await Assert.That(result.Command.Arguments[key]).IsEqualTo(expected);
            }

            return;
        }

        await Assert.That(result.Success).IsFalse();
        await Assert.That(testCase.ExpectedFailure).IsNotNull();

        var codes = result.Failures.Select(f => f.Code).ToArray();
        await Assert.That(codes).Contains(testCase.ExpectedFailure!.Value);

        if (testCase.MinimumCandidateCount is int min)
            await Assert.That(result.Candidates.Count).IsGreaterThanOrEqualTo(min);
    }

    public static async Task AssertParserCase(
        CommandParser parser,
        ParseCase testCase,
        string? explicitContext,
        IReadOnlyDictionary<string, string>? verbAliases = null)
    {
        var trimmed = testCase.Prompt.Trim();
        var bodyTokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var needsContextPrefix = explicitContext is not null &&
            (bodyTokens.Length == 0 ||
             !string.Equals(bodyTokens[0], explicitContext, StringComparison.OrdinalIgnoreCase));
        var normalizedPrompt = needsContextPrefix && explicitContext is not null
            ? $"{explicitContext} {trimmed}"
            : trimmed;
        var normalized = normalizedPrompt.ToLowerInvariant();
        var tokens = CommandTokenizer.Tokenize(normalized);
        var result = parser.Parse(
            normalizedPrompt,
            normalized,
            tokens,
            explicitContext,
            verbAliases ?? EmptyAliases);

        if (testCase.ExpectSuccess)
        {
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Command!.Name).IsEqualTo(testCase.ExpectedCommandName!);
            if (testCase.ExpectedArguments is not null)
            {
                foreach (var (key, expected) in testCase.ExpectedArguments)
                    await Assert.That(result.Command.Arguments[key]).IsEqualTo(expected);
            }

            return;
        }

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures.Any(f => f.Code == testCase.ExpectedFailure)).IsTrue();
    }

    private static IReadOnlyDictionary<string, string> EmptyAliases { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
