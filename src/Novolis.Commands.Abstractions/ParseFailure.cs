namespace Novolis.Commands;

/// <summary>ParseFailure operation.</summary>
/// <summary>Represents ParseFailure.</summary>
public sealed record ParseFailure(
    /// <summary>Fragment.</summary>
    ParseFailureCode Code,
    string Message,
    string? Fragment = null);
