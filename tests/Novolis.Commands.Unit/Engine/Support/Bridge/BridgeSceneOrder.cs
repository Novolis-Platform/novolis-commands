namespace Novolis.Commands.Engine.Tests.Support.Bridge;

/// <summary>
/// One beat in a scripted bridge scene: what the captain (or crew) says and what the computer should parse.
/// </summary>
public sealed record BridgeSceneOrder(
    string Beat,
    string Spoken,
    string? ExpectedCommand = null,
    IReadOnlyDictionary<string, object?>? Arguments = null,
    string? ExpectedContext = null,
    string? Speaker = null,
    bool ShouldFail = false,
    ParseFailureCode? ExpectedFailure = null,
    CommandPriority? ExpectedPriority = null,
    bool? Interrupts = null);
