namespace Novolis.Commands.Engine.Tests.Support;

public sealed record ParseCase(
    string Id,
    string Prompt,
    bool ExpectSuccess,
    string? ExpectedCommandName = null,
    ParseFailureCode? ExpectedFailure = null,
    string? ExpectedContext = null,
    IReadOnlyDictionary<string, object?>? ExpectedArguments = null,
    int? MinimumCandidateCount = null);
