using Novolis.Commands;
using Novolis.Commands.Engine;
using Novolis.Commands.Queueing;
using Novolis.Commands.Testing;
using TUnit.Assertions;

namespace Novolis.Commands.Engine.Tests.Support.Bridge;

internal static class BridgeSceneRunner
{
    public static CommandEngine<TestCommandContext> CreateEngine() =>
        new(CommandEngineTestRegistry.CreateBridge(), new TestCommandContextResolver());

    public static async Task PlayParseSceneAsync(BridgeScene scene)
    {
        var engine = CreateEngine();
        var context = TestContexts.Bridge;

        foreach (var order in scene.Orders)
        {
            var result = await engine.ParseCommandAsync(order.Spoken, context);

            if (order.ShouldFail)
            {
                await Assert.That(result.Success).IsFalse();
                if (order.ExpectedFailure is ParseFailureCode code)
                    await Assert.That(result.Failures.Any(f => f.Code == code)).IsTrue();
                continue;
            }

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Command!.Name).IsEqualTo(order.ExpectedCommand!);

            if (order.ExpectedContext is not null)
                await Assert.That(result.Command.ContextWord).IsEqualTo(order.ExpectedContext);

            if (order.Arguments is not null)
            {
                foreach (var (key, value) in order.Arguments)
                {
                    if (value is double or int or float)
                        await Assert.That(Convert.ToDouble(result.Command.Arguments[key])).IsEqualTo(Convert.ToDouble(value));
                    else
                        await Assert.That(result.Command.Arguments[key]).IsEqualTo(value);
                }
            }

            if (order.ExpectedPriority is CommandPriority priority)
                await Assert.That(result.Command.Priority).IsEqualTo(priority);

            if (order.Interrupts is bool interrupts)
                await Assert.That(result.Command.InterruptsCurrentCommand).IsEqualTo(interrupts);
        }
    }

    public static async Task PlaySimulationAsync(BridgeScene scene, CancellationToken cancellationToken = default)
    {
        var engine = CreateEngine();
        var bridge = new BridgeSimulator();
        var context = TestContexts.Bridge;

        foreach (var order in scene.Orders)
        {
            if (order.ShouldFail)
            {
                var fail = await engine.ParseCommandAsync(order.Spoken, context);
                await Assert.That(fail.Success).IsFalse();
                continue;
            }

            var parsed = await engine.ParseCommandAsync(order.Spoken, context);
            await Assert.That(parsed.Success).IsTrue();
            await bridge.ProcessAsync(parsed.Command!, cancellationToken);
        }

        if (scene.EndState is BridgeSceneExpectation end)
            await AssertEndStateAsync(bridge, end);
    }

    /// <summary>
    /// Runs a scene through the real queue runner (for belay / interrupt behaviour).
    /// </summary>
    public static async Task PlayQueuedSceneAsync(
        BridgeScene scene,
        CancellationToken cancellationToken = default)
    {
        var engine = CreateEngine();
        var bridge = new BridgeSimulator();
        var queue = new ManualCommandQueue();
        var processor = new BridgeSimulatorProcessor(bridge);
        var runner = new CommandQueueRunner<TestCommandContext>(queue, processor);
        var context = TestContexts.Bridge;

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var runTask = runner.RunAsync(context, runCts.Token);

        foreach (var order in scene.Orders.Where(o => !o.ShouldFail))
        {
            var parsed = await engine.ParseCommandAsync(order.Spoken, context);
            await Assert.That(parsed.Success).IsTrue();
            await queue.EnqueueAsync(parsed.Command!, cancellationToken);
            await Task.Delay(20, cancellationToken);
        }

        await Task.Delay(100, cancellationToken);
        runCts.Cancel();
        try
        {
            await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
        }

        if (scene.EndState is BridgeSceneExpectation end)
            await AssertEndStateAsync(bridge, end);
    }

    public static async Task AssertEndStateAsync(BridgeSimulator bridge, BridgeSceneExpectation expected)
    {
        if (expected.Heading is double h)
            await Assert.That(bridge.Heading).IsEqualTo(h);

        if (expected.HeadingBy is double by)
            await Assert.That(bridge.HeadingBy).IsEqualTo(by);

        if (expected.Warp is int w)
            await Assert.That(bridge.SpeedWarp).IsEqualTo(w);

        if (expected.ShieldsMin is int shields)
            await Assert.That(bridge.ShieldPercent).IsGreaterThanOrEqualTo(shields);

        if (expected.HullMin is int hull)
            await Assert.That(bridge.HullPercent).IsGreaterThanOrEqualTo(hull);

        if (expected.TargetLocked is bool locked)
            await Assert.That(bridge.TargetLocked).IsEqualTo(locked);

        if (expected.TargetName is not null)
            await Assert.That(bridge.TargetName).IsEqualTo(expected.TargetName);

        if (expected.StatusContains is not null)
            await Assert.That(bridge.StatusLine).Contains(expected.StatusContains);
    }

    private sealed class BridgeSimulatorProcessor(BridgeSimulator bridge) : ICommandProcessor<TestCommandContext>
    {
        public ValueTask ProcessAsync(
            CommandEnvelope command,
            TestCommandContext context,
            CancellationToken cancellationToken) =>
            bridge.ProcessAsync(command, cancellationToken);
    }
}
