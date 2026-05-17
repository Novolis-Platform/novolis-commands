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
                verbs: ["heading", "set heading"],
                CommandArgumentDefinition.Integer("heading", required: true))
            .Add("helm.full-stop", "helm", ["full stop", "all stop"])
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
    public async Task ParseCommandAsync_Should_Parse_MultiWord_Phrase()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("helm full stop", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo("helm.full-stop");
    }

    [Test]
    public async Task ParseCommandAsync_Should_Parse_Set_Heading_Phrase()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("helm set heading 180", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Arguments["heading"]).IsEqualTo(180);
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
    public async Task ParseCommandAsync_Should_Parse_Help()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("help tactical", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo(BuiltInCommands.Help);
        await Assert.That(result.Command.Arguments["topic"]).IsEqualTo("tactical");
    }

    [Test]
    public async Task ParseCommandAsync_Should_Return_UnknownContext_Without_Prefix()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("heading 270", context);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.UnknownContext);
    }

    [Test]
    public async Task ParseCommandAsync_Should_Return_Failure_For_Unknown_Command()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext();

        var result = await engine.ParseCommandAsync("helm do the thing", context);

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
    public async Task ParseCommandAsync_Should_Resolve_Context_Alias()
    {
        var engine = CreateEngine(HelmRegistry());
        var context = new TestCommandContext
        {
            ContextAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["pilot"] = "helm"
            }
        };

        var result = await engine.ParseCommandAsync("pilot heading 90", context);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.ContextWord).IsEqualTo("helm");
    }

    [Test]
    public async Task ParseCommandAsync_Should_Parse_Nav_MultiWord_Destination()
    {
        var registry = new CommandRegistryBuilder()
            .Add("nav.set-course", "nav", ["set course", "course"],
                CommandArgumentDefinition.String("destination", required: true))
            .Build();

        var engine = CreateEngine(registry);
        var result = await engine.ParseCommandAsync("nav set course alpha centauri", new TestCommandContext());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Arguments["destination"]).IsEqualTo("alpha centauri");
    }
}
