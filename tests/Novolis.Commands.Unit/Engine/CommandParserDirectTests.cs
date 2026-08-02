using Novolis.Commands.Engine;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandParserDirectTests
{
    private static CommandParser CreateParser(
        ICommandRegistry registry,
        IReadOnlyDictionary<string, ICommandArgumentParser>? argumentParsers = null) =>
        new(registry, argumentParsers);

    [Test]
    public async Task Parse_Unknown_Context_Returns_Failure()
    {
        var registry = new CommandRegistryBuilder()
            .Add("helm.stop", "helm", ["full stop"])
            .Build();

        var parser = CreateParser(registry);
        var tokens = new[] { "unknown", "full", "stop" };
        var result = parser.Parse("unknown full stop", "unknown full stop", tokens, "unknown", new Dictionary<string, string>());

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.UnknownContext);
    }

    [Test]
    public async Task Parse_Missing_Argument_Parser_Returns_Failure()
    {
        var registry = new CommandRegistryBuilder()
            .Add("lab.echo", "lab", ["echo"], "missing-parser")
            .Build(validate: false);

        var parser = CreateParser(registry);
        var result = parser.Parse("lab echo hi", "lab echo hi", ["lab", "echo", "hi"], "lab", new Dictionary<string, string>());

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.InvalidArgument);
    }

    [Test]
    public async Task Parse_Double_Argument_Succeeds()
    {
        var registry = new CommandRegistryBuilder()
            .Add("scale.factor", null, ["scale"],
                CommandArgumentDefinition.Double("factor", required: true))
            .Build();

        var parser = CreateParser(registry);
        var result = parser.Parse("scale 2", "scale 2", ["scale", "2"], null, new Dictionary<string, string>());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Arguments["factor"]).IsEqualTo(2.0);
    }

    [Test]
    public async Task Parse_Invalid_Double_Returns_Failure()
    {
        var registry = new CommandRegistryBuilder()
            .Add("scale.factor", null, ["scale"],
                CommandArgumentDefinition.Double("factor", required: true))
            .Build();

        var parser = CreateParser(registry);
        var result = parser.Parse("scale abc", "scale abc", ["scale", "abc"], null, new Dictionary<string, string>());

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.InvalidArgument);
    }

    [Test]
    public async Task Parse_Verb_Alias_Expands_Registered_Shorthand()
    {
        var registry = new CommandRegistryBuilder()
            .Add("helm.stop", "helm", ["fs"])
            .Build();

        var parser = CreateParser(registry);
        var aliases = new Dictionary<string, string> { ["fs"] = "full stop" };
        var result = parser.Parse("helm full stop", "helm full stop", ["helm", "full", "stop"], "helm", aliases);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo("helm.stop");
    }

    [Test]
    public async Task Parse_Custom_Parser_Failure_Propagates()
    {
        var registry = new CommandRegistryBuilder()
            .Add("lab.echo", "lab", ["echo"], "echo")
            .Build(validate: false);

        var parsers = new Dictionary<string, ICommandArgumentParser>
        {
            ["echo"] = new FailingParser()
        };

        var parser = CreateParser(registry, parsers);
        var result = parser.Parse("lab echo bad", "lab echo bad", ["lab", "echo", "bad"], "lab", new Dictionary<string, string>());

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Failures[0].Code).IsEqualTo(ParseFailureCode.MissingArgument);
    }

    [Test]
    public async Task CommandArgumentDefinition_Factories_Set_Kinds()
    {
        await Assert.That(CommandArgumentDefinition.Integer("n").Kind).IsEqualTo(CommandArgumentKind.Integer);
        await Assert.That(CommandArgumentDefinition.String("s", required: false).Required).IsFalse();
        await Assert.That(CommandArgumentDefinition.Double("d").Kind).IsEqualTo(CommandArgumentKind.Double);
    }

    [Test]
    public async Task CommandSuggestionService_Returns_Closest_Phrases()
    {
        var registry = new CommandRegistryBuilder()
            .Add("helm.stop", "helm", ["full stop", "all stop"])
            .Build();

        var suggestions = new CommandSuggestionService().Suggest(
            ["ful", "stop"],
            verbStartIndex: 0,
            contextWord: "helm",
            registry,
            maxSuggestions: 2);

        await Assert.That(suggestions).IsNotEmpty();
        await Assert.That(suggestions[0]).IsEqualTo("full stop");
    }

    private sealed class FailingParser : ICommandArgumentParser
    {
        public bool TryParse(
            CommandDefinition definition,
            IReadOnlyList<string> argumentTokens,
            out IReadOnlyDictionary<string, object?> arguments,
            out ParseFailure? failure)
        {
            arguments = new Dictionary<string, object?>();
            failure = new ParseFailure(ParseFailureCode.MissingArgument, "Need text.", "text");
            return false;
        }
    }
}
