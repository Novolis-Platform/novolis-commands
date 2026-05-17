using Novolis.Commands.Engine.Tests.Support.Bridge;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

/// <summary>
/// Full bridge scenes: parse → queue → execute, with ship state verified at the end.
/// </summary>
public sealed class BridgeSceneSimulationTests
{
    [Test]
    public async Task Kr12Incident_Full_Scene_Should_Leave_Ship_In_Combat_Ready_State()
    {
        var scene = BridgeScenes.Kr12RedAlertEncounter();
        await BridgeSceneRunner.PlaySimulationAsync(scene);

        await Assert.That(scene.Title).Contains("KR-12");
    }

    [Test]
    public async Task AlphaCentauriApproach_Should_Hail_Then_All_Stop()
    {
        await BridgeSceneRunner.PlaySimulationAsync(BridgeScenes.AlphaCentauriFirstContact());
    }

    [Test]
    public async Task DamageControl_Should_Repair_Hull_And_Fire_At_Locked_Target()
    {
        await BridgeSceneRunner.PlaySimulationAsync(BridgeScenes.DamageControlAfterExchange());
    }

    [Test]
    public async Task NightWatch_Should_Set_Cruise_Heading_And_Warp()
    {
        await BridgeSceneRunner.PlaySimulationAsync(BridgeScenes.QuietNightWatch());
    }

    [Test]
    public async Task PersonnelTransfer_Should_Log_Admin_Order()
    {
        await BridgeSceneRunner.PlaySimulationAsync(BridgeScenes.PersonnelTransfer());
    }

    [Test]
    public async Task NaturalOrdersFromLog_Should_Parse_And_Execute()
    {
        await BridgeSceneRunner.PlaySimulationAsync(BridgeScenes.NaturalOrdersFromLog());
    }

    [Test]
    public async Task Kr12Scene_Log_Should_Read_Like_A_Transcript()
    {
        var bridge = new BridgeSimulator();
        var engine = BridgeSceneRunner.CreateEngine();
        var context = Support.TestContexts.Bridge;

        foreach (var order in BridgeScenes.Kr12RedAlertEncounter().Orders)
        {
            var parsed = await engine.ParseCommandAsync(order.Spoken, context);
            await Assert.That(parsed.Success).IsTrue();
            await bridge.ProcessAsync(parsed.Command!);
        }

        await Assert.That(bridge.Log.Count).IsGreaterThanOrEqualTo(12);
        await Assert.That(string.Join('\n', bridge.Log)).Contains("engineering divert shields");
        await Assert.That(string.Join('\n', bridge.Log)).Contains("weapons fired");
    }
}
