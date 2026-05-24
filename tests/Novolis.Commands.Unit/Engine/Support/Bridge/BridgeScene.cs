namespace Novolis.Commands.Engine.Tests.Support.Bridge;

public sealed record BridgeScene(
    string Title,
    string Epigraph,
    IReadOnlyList<BridgeSceneOrder> Orders,
    BridgeSceneExpectation? EndState = null);

public sealed record BridgeSceneExpectation(
    double? Heading = null,
    double? HeadingBy = null,
    int? Warp = null,
    int? ShieldsMin = null,
    int? HullMin = null,
    bool? TargetLocked = null,
    string? TargetName = null,
    string? StatusContains = null);
