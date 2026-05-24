namespace Novolis.Commands;

/// <summary>A single parse failure with code, message, and optional source fragment.</summary>
/// <param name="Code">Failure classification.</param>
/// <param name="Message">Human-readable message.</param>
/// <param name="Fragment">Substring of the prompt related to the failure.</param>
public sealed record ParseFailure(
    ParseFailureCode Code,
    string Message,
    string? Fragment = null);
