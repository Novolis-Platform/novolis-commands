namespace Novolis.Commands.Engine.Tests.Support.Bridge;

public sealed record BridgeScene(
    string Title,
    string Epigraph,
    IReadOnlyList<BridgeSceneOrder> Orders,
    BridgeSceneExpectation? EndState = null);
