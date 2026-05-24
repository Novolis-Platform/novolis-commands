namespace Novolis.Commands;

/// <summary>CommandCandidate operation.</summary>
/// <summary>Represents CommandCandidate.</summary>
public sealed record CommandCandidate(
    /// <summary>Arguments.</summary>
    string Name,
    /// <summary>Reason.</summary>
    string? ContextWord,
    IReadOnlyDictionary<string, object?> Arguments,
    double Confidence,
    string Reason);
