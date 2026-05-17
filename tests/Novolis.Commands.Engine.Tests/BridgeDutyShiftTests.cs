using Novolis.Commands.Engine.Tests.Support;
using Novolis.Commands.Engine.Tests.Support.Bridge;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

/// <summary>
/// Multi-scene duty shift: the same crew, one continuous watch.
/// </summary>
public sealed class BridgeDutyShiftTests
{
    [Test]
    public async Task Full_Duty_Shift_From_Encounter_To_Night_Watch()
    {
        var bridge = new BridgeSimulator();
        var engine = BridgeSceneRunner.CreateEngine();
        var context = TestContexts.Bridge;

        var scenes = new[]
        {
            BridgeScenes.Kr12RedAlertEncounter(),
            BridgeScenes.DamageControlAfterExchange(),
            BridgeScenes.QuietNightWatch()
        };

        foreach (var scene in scenes)
        {
            foreach (var order in scene.Orders.Where(o => !o.ShouldFail))
            {
                var parsed = await engine.ParseCommandAsync(order.Spoken, context);
                await Assert.That(parsed.Success).IsTrue();
                await bridge.ProcessAsync(parsed.Command!);
            }
        }

        await Assert.That(bridge.TargetLocked).IsTrue();
        await Assert.That(bridge.TargetName).IsEqualTo("Hostile frigate KR-12");
        await Assert.That(bridge.Heading).IsEqualTo(180);
        await Assert.That(bridge.SpeedWarp).IsEqualTo(5);
        await Assert.That(bridge.ShieldPercent).IsGreaterThanOrEqualTo(85);
        await Assert.That(string.Join('\n', bridge.Log)).Contains("conn warp 8");
        await Assert.That(string.Join('\n', bridge.Log)).Contains("weapons fired");
    }
}
