namespace Novolis.Commands;

public sealed record ParseResult
{
    public required bool Success { get; init; }
    public CommandEnvelope? Command { get; init; }
    public IReadOnlyList<ParseFailure> Failures { get; init; } = [];
    public IReadOnlyList<CommandCandidate> Candidates { get; init; } = [];

    /// <summary>
    /// Close registered verb phrases when parsing fails with <see cref="ParseFailureCode.UnknownCommand"/>.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    public static ParseResult Succeeded(CommandEnvelope command) =>
        new() { Success = true, Command = command };

    public static ParseResult Failed(params ParseFailure[] failures) =>
        Failed((IReadOnlyList<ParseFailure>)failures);

    public static ParseResult Failed(
        IReadOnlyList<ParseFailure> failures,
        IReadOnlyList<string>? suggestions = null) =>
        new()
        {
            Success = false,
            Failures = failures,
            Suggestions = suggestions ?? []
        };
}
