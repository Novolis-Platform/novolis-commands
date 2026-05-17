namespace Novolis.Commands;

public sealed record ParseResult
{
    public required bool Success { get; init; }
    public CommandEnvelope? Command { get; init; }
    public IReadOnlyList<ParseFailure> Failures { get; init; } = [];
    public IReadOnlyList<CommandCandidate> Candidates { get; init; } = [];

    public static ParseResult Succeeded(CommandEnvelope command) =>
        new() { Success = true, Command = command };

    public static ParseResult Failed(params ParseFailure[] failures) =>
        new() { Success = false, Failures = failures };
}
