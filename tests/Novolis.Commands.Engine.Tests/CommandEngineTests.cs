using Novolis.Commands.Engine;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public class CommandEngineTests
{
    private static CommandEngine<TestCommandContext> CreateEngine(ICommandRegistry registry) =>
        new(registry, new TestCommandContextResolver());

    private static ICommandRegistry HelmRegistry() =>
        new CommandRegistryBuilder()
            .Add(
                "helm.set-heading",
                context: "helm",
                verbs: ["heading"],
                CommandArgumentDefinition.Integer("heading", required: true))
            .Build();

    [Test]
    public async Task ParseCommandAsync_Should_Parse_Helm_Command()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("helm heading 270", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo("helm.set-heading");
        await Assert.That(result.Command.Arguments["heading"]).IsEqualTo(270);
    }

    [Test]
    public async Task ParseCommandAsync_Should_Parse_Belay_That_As_Emergency_Interrupt()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("belay that", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo(BuiltInCommands.BelayThat);
        await Assert.That(result.Command.Priority).IsEqualTo(CommandPriority.Emergency);
        await Assert.That(result.Command.InterruptsCurrentCommand).IsTrue();
    }

    [Test]
    public async Task ParseCommandAsync_Should_Return_Failure_For_Unknown_Command()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("do the thing", context);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures.Any(x => x.Code == ParseFailureCode.UnknownCommand)).IsTrue();
    }

    [Test]
    public async Task ParseCommandAsync_Should_Return_EmptyPrompt_Failure()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("   ", context);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.EmptyPrompt);
    }

    [Test]
    public async Task ParseCommandAsync_Should_Return_Ambiguous_Command_With_Candidates()
    {
        var registry = new CommandRegistryBuilder()
            .Add("tactical.fire-weapons", "tactical", ["fire"])
            .Add("crew.dismiss-personnel", "admin", ["fire"])
            .Build();

        var engine = CreateEngine(registry);
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("fire", context);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.AmbiguousCommand);
        await Assert.That(result.Candidates.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(result.Candidates[0].Name).IsEqualTo("tactical.fire-weapons");
    }
}
