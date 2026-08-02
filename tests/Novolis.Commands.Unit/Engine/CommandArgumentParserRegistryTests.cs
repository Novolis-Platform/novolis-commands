using Novolis.Commands.Engine;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandArgumentParserRegistryTests
{
    [Test]
    public async Task Register_Should_Validate_And_Expose_Parsers()
    {
        var registry = new CommandArgumentParserRegistry();
        var parser = new EchoParser();

        await Assert.That(() => registry.Register("", parser)).Throws<ArgumentException>();
        await Assert.That(() => registry.Register("echo", null!)).Throws<ArgumentNullException>();

        registry.Register("echo", parser);
        await Assert.That(registry.TryGet("echo", out var found)).IsTrue();
        await Assert.That(found).IsSameReferenceAs(parser);
        await Assert.That(registry.Keys).Contains("echo");
    }

    [Test]
    public async Task Engine_Should_Parse_With_Registered_Custom_Parser()
    {
        var registry = new CommandRegistryBuilder()
            .Add("lab.echo", "lab", ["echo"], "echo")
            .Build();

        var options = new CommandEngineOptions();
        options.ArgumentParsers.Register("echo", new EchoParser());

        var engine = new CommandEngine<TestCommandContext>(
            registry,
            new TestCommandContextResolver(),
            options);

        var result = await engine.ParseCommandAsync("lab echo hello", new TestCommandContext());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Arguments["text"]).IsEqualTo("hello");
    }

    private sealed class EchoParser : ICommandArgumentParser
    {
        public bool TryParse(
            CommandDefinition definition,
            IReadOnlyList<string> argumentTokens,
            out IReadOnlyDictionary<string, object?> arguments,
            out ParseFailure? failure)
        {
            failure = null;
            arguments = new Dictionary<string, object?>
            {
                ["text"] = string.Join(" ", argumentTokens)
            };
            return true;
        }
    }
}
