namespace Novolis.Commands.Engine.Tests.Support;

/// <summary>
/// Parser-level cases (context already resolved). Complements engine integration cases.
/// </summary>
public static class CommandParserTestCases
{
    public static IEnumerable<ParseCase> SuccessCases()
    {
        for (var n = 0; n <= 99; n++)
        {
            yield return Case(
                $"parser-int-{n}",
                $"heading {n}",
                true,
                "test.counter",
                "lab",
                new Dictionary<string, object?> { ["value"] = n });
        }

        foreach (var label in SampleLabels())
        {
            yield return Case(
                $"parser-str-{label.Replace(' ', '-')}",
                $"label {label}",
                true,
                "test.label",
                "lab",
                new Dictionary<string, object?> { ["text"] = label });
        }

        yield return Case("parser-optional-missing", "ping", true, "test.ping", "lab");
        yield return Case("parser-optional-present", "ping 42", true, "test.ping", "lab",
            new Dictionary<string, object?> { ["code"] = 42 });
    }

    public static IEnumerable<ParseCase> FailureCases()
    {
        yield return Case("parser-no-verb", "", false, expectedFailure: ParseFailureCode.UnknownCommand);
        yield return Case("parser-unknown", "explode", false, expectedFailure: ParseFailureCode.UnknownCommand);
        yield return Case("parser-missing-int", "heading", false, expectedFailure: ParseFailureCode.MissingArgument);
        yield return Case("parser-bad-int", "heading xy", false, expectedFailure: ParseFailureCode.InvalidArgument);
        yield return Case("parser-extra", "heading 1 2 3", false, expectedFailure: ParseFailureCode.InvalidArgument);

        foreach (var bad in new[] { "abc", "12.5", "1e3", "0x10" })
        {
            yield return Case(
                $"parser-bad-int-{bad.Replace('.', '-')}",
                $"heading {bad}",
                false,
                expectedFailure: ParseFailureCode.InvalidArgument);
        }
    }

    public static IEnumerable<ParseCase> AmbiguousCases() =>
        CommandEngineTestCases.AmbiguousCases();

    public static ICommandRegistry ParserRegistry() =>
        new CommandRegistryBuilder()
            .Add("test.counter", "lab", ["heading"],
                CommandArgumentDefinition.Integer("value", required: true))
            .Add("test.label", "lab", ["label"],
                CommandArgumentDefinition.String("text", required: true))
            .Add("test.ping", "lab", ["ping"],
                CommandArgumentDefinition.Integer("code", required: false))
            .Add("dup.a", "shared", ["scan"])
            .Add("dup.b", "shared", ["scan"])
            .Add("dup.c", "shared", ["lock target"])
            .Add("dup.d", "shared", ["lock target"])
            .Build(validate: false);

    private static IEnumerable<string> SampleLabels() =>
        Enumerable.Range(1, 40).Select(i => $"sample label number {i}");

    private static ParseCase Case(
        string id,
        string prompt,
        bool success,
        string? commandName = null,
        string? context = null,
        IReadOnlyDictionary<string, object?>? arguments = null,
        ParseFailureCode? expectedFailure = null,
        int? minimumCandidateCount = null) =>
        new(id, prompt, success, commandName, expectedFailure, context, arguments, minimumCandidateCount);
}
