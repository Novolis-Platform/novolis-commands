namespace Novolis.Commands;

/// <summary>Alternative parse candidate when the prompt is ambiguous.</summary>
/// <param name="Name">Candidate command name.</param>
/// <param name="ContextWord">Optional context word.</param>
/// <param name="Arguments">Parsed arguments for this candidate.</param>
/// <param name="Confidence">Match confidence in the range 0–1.</param>
/// <param name="Reason">Why this candidate was considered.</param>
public sealed record CommandCandidate(
    string Name,
    string? ContextWord,
    IReadOnlyDictionary<string, object?> Arguments,
    double Confidence,
    string Reason);
