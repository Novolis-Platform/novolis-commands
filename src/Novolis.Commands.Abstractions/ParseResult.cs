namespace Novolis.Commands;

/// <summary>Outcome of parsing a user prompt.</summary>
public sealed record ParseResult
{
    /// <summary>Whether parsing produced a command.</summary>
    public required bool Success { get; init; }

    /// <summary>Parsed command when <see cref="Success"/> is true.</summary>
    public CommandEnvelope? Command { get; init; }

    /// <summary>Structured parse failures when <see cref="Success"/> is false.</summary>
    public IReadOnlyList<ParseFailure> Failures { get; init; } = [];

    /// <summary>Alternative command candidates considered during parsing.</summary>
    public IReadOnlyList<CommandCandidate> Candidates { get; init; } = [];

    /// <summary>
    /// Close registered verb phrases when parsing fails with <see cref="ParseFailureCode.UnknownCommand"/>.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    /// <summary>Creates a successful parse result.</summary>
    /// <param name="command">Parsed command envelope.</param>
    public static ParseResult Succeeded(CommandEnvelope command) =>
        new() { Success = true, Command = command };

    /// <summary>Creates a failed parse result.</summary>
    /// <param name="failures">One or more failures.</param>
    public static ParseResult Failed(params ParseFailure[] failures) =>
        Failed((IReadOnlyList<ParseFailure>)failures);

    /// <summary>Creates a failed parse result with optional suggestions.</summary>
    /// <param name="failures">Parse failures.</param>
    /// <param name="suggestions">Suggested command phrases.</param>
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
