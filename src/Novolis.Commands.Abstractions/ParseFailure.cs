namespace Novolis.Commands;

public sealed record ParseFailure(
    ParseFailureCode Code,
    string Message,
    string? Fragment = null);
