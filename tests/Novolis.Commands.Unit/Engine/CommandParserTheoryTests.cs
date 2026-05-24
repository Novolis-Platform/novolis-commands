using Novolis.Commands.Engine;
using Novolis.Commands.Engine.Tests.Support;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandParserTheoryTests
{
    private static CommandParser CreateParser(ICommandRegistry? registry = null) =>
        new(registry ?? CommandParserTestCases.ParserRegistry());

    public static IEnumerable<ParseCase> SuccessCases() => CommandParserTestCases.SuccessCases();

    public static IEnumerable<ParseCase> FailureCases() => CommandParserTestCases.FailureCases();

    public static IEnumerable<ParseCase> AmbiguousCases() => CommandParserTestCases.AmbiguousCases();

    [Test]
    [MethodDataSource(nameof(SuccessCases))]
    public async Task Parse_WithLabContext_Should_Succeed(ParseCase testCase)
    {
        var parser = CreateParser();
        await ParseAssertions.AssertParserCase(parser, testCase, explicitContext: "lab");
    }

    [Test]
    [MethodDataSource(nameof(FailureCases))]
    public async Task Parse_WithLabContext_Should_Fail(ParseCase testCase)
    {
        var parser = CreateParser();
        var context = testCase.Id.StartsWith("parser-no-verb", StringComparison.Ordinal)
            ? (string?)null
            : "lab";
        await ParseAssertions.AssertParserCase(parser, testCase, context);
    }

    [Test]
    [MethodDataSource(nameof(AmbiguousCases))]
    public async Task Parse_SharedContext_Should_Be_Ambiguous(ParseCase testCase)
    {
        var parser = CreateParser();
        await ParseAssertions.AssertParserCase(parser, testCase, explicitContext: "shared");
    }
}
