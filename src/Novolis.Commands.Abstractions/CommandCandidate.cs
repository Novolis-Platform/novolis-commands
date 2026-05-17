namespace Novolis.Commands;

public sealed record CommandCandidate(
    string Name,
    string? ContextWord,
    IReadOnlyDictionary<string, object?> Arguments,
    double Confidence,
    string Reason);
