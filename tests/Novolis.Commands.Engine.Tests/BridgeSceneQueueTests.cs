using Novolis.Commands.Engine.Tests.Support.Bridge;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

/// <summary>
/// Bridge scenes that depend on the command queue (belay interrupts in-flight work).
/// </summary>
public sealed class BridgeSceneQueueTests
{
    [Test]
    public async Task EvasiveManeuvers_Belay_Should_Interrupt_Through_Queue()
    {
        await BridgeSceneRunner.PlayQueuedSceneAsync(BridgeScenes.EvasiveManeuversAndBelay());
    }
}
