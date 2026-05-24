using Novolis.Commands.Engine;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandRegistryBuilderTests
{
    public static IEnumerable<string> ContextNames() =>
    [
        "helm", "tactical", "engineering", "nav", "comms", "science", "security", "medical"
    ];

    [Test]
    [MethodDataSource(nameof(ContextNames))]
    public async Task Build_Should_Expose_All_Registered_Contexts(string context)
    {
        var registry = new CommandRegistryBuilder()
            .Add($"cmd.{context}", context, ["go"])
            .Build();

        var definitions = registry.GetAll().ToList();
        await Assert.That(definitions.Count).IsEqualTo(1);
        await Assert.That(definitions[0].ContextWord).IsEqualTo(context);
        await Assert.That(definitions[0].Name).IsEqualTo($"cmd.{context}");
    }

    [Test]
    public async Task Build_Should_Preserve_Multiple_Verbs()
    {
        var registry = new CommandRegistryBuilder()
            .Add("test.cmd", "ctx", ["alpha", "beta gamma"])
            .Build();

        var verbs = registry.GetAll().Single().Verbs;
        await Assert.That(verbs).IsEquivalentTo(["alpha", "beta gamma"]);
    }

    [Test]
    public async Task Build_Should_Preserve_Argument_Definitions()
    {
        var registry = new CommandRegistryBuilder()
            .Add(
                "test.args",
                "ctx",
                ["run"],
                CommandArgumentDefinition.Integer("count", required: true),
                CommandArgumentDefinition.String("label", required: false))
            .Build();

        var args = registry.GetAll().Single().Arguments;
        await Assert.That(args.Count).IsEqualTo(2);
        await Assert.That(args[0].Name).IsEqualTo("count");
        await Assert.That(args[0].Required).IsTrue();
        await Assert.That(args[1].Name).IsEqualTo("label");
        await Assert.That(args[1].Required).IsFalse();
    }
}
