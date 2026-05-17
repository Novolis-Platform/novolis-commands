using Novolis.Commands.Engine;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandSuggestionTests
{
    [Test]
    public async Task ParseCommandAsync_Should_Suggest_Close_Verb_Phrases()
    {
        var registry = new CommandRegistryBuilder()
            .Add("helm.full-stop", "helm", ["full stop", "all stop"])
            .Build();

        var engine = new CommandEngine<TestCommandContext>(registry, new TestCommandContextResolver());
        var result = await engine.ParseCommandAsync("helm ful stop", new TestCommandContext());

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.UnknownCommand);
        await Assert.That(result.Suggestions).IsNotEmpty();
        await Assert.That(result.Suggestions[0]).IsEqualTo("full stop");
    }
}
