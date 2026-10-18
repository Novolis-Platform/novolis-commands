namespace Novolis.Commands.Navigation;

/// <summary>One structural diagnostic reported for an invalid menu tree.</summary>
public sealed record MobileMenuTreeDiagnostic(
    string Code,
    string Message,
    string? NodeId = null);
