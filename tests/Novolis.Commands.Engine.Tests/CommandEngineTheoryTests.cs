using Novolis.Commands.Engine;
using Novolis.Commands.Engine.Tests.Support;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandEngineTheoryTests
{
    private static CommandEngine<TestCommandContext> CreateEngine(ICommandRegistry? registry = null) =>
        new(registry ?? CommandEngineTestRegistry.Create(), new TestCommandContextResolver());

    public static IEnumerable<ParseCase> SuccessCases() => CommandEngineTestCases.SuccessCases();

    public static IEnumerable<ParseCase> FailureCases() => CommandEngineTestCases.FailureCases();

    public static IEnumerable<ParseCase> AmbiguousCases() => CommandEngineTestCases.AmbiguousCases();

    [Test]
    [MethodDataSource(nameof(SuccessCases))]
    public async Task Parse_Should_Succeed(ParseCase testCase)
    {
        var engine = CreateEngine();
        await ParseAssertions.AssertEngineCase(engine, TestContexts.Bridge, testCase);
    }

    [Test]
    [MethodDataSource(nameof(FailureCases))]
    public async Task Parse_Should_Fail(ParseCase testCase)
    {
        var engine = CreateEngine();
        await ParseAssertions.AssertEngineCase(engine, TestContexts.Bridge, testCase);
    }

    [Test]
    [MethodDataSource(nameof(AmbiguousCases))]
    public async Task Parse_Should_Report_Ambiguous(ParseCase testCase)
    {
        var engine = CreateEngine(AmbiguousCommandRegistry.Create());
        await ParseAssertions.AssertEngineCase(engine, TestContexts.Empty, testCase);
    }
}
