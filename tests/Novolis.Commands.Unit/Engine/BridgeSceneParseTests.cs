using Novolis.Commands.Engine.Tests.Support.Bridge;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

/// <summary>
/// Every spoken line in our Star Trek bridge scripts must parse exactly as the crew intended.
/// </summary>
public sealed class BridgeSceneParseTests
{
    public static IEnumerable<BridgeScene> Scenes() => BridgeScenes.All();

    public static IEnumerable<BridgeSceneOrder> EveryOrder() => BridgeScenes.AllOrders();

    [Test]
    [MethodDataSource(nameof(Scenes))]
    public async Task Scene_Every_Line_Should_Parse(BridgeScene scene)
    {
        await BridgeSceneRunner.PlayParseSceneAsync(scene);
    }

    [Test]
    [MethodDataSource(nameof(EveryOrder))]
    public async Task Order_Should_Parse_In_Isolation(BridgeSceneOrder order)
    {
        var scene = new BridgeScene(
            Title: order.Beat,
            Epigraph: order.Speaker is null ? "" : $"{order.Speaker} on the bridge.",
            Orders: [order]);

        await BridgeSceneRunner.PlayParseSceneAsync(scene);
    }

    [Test]
    public async Task Kr12Incident_Should_Match_Bridge_Help_Example()
    {
        var scene = BridgeScenes.Kr12RedAlertEncounter();

        await Assert.That(scene.Orders.Select(o => o.Spoken)).Contains("tactical lock target");
        await Assert.That(scene.Orders.Select(o => o.Spoken)).Contains("weaps fire");
        await Assert.That(scene.Orders.Select(o => o.Spoken)).Contains("engineering divert shields");
    }
}
