using Novolis.Commands.Engine;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class CommandRegistryValidatorTests
{
    [Test]
    public async Task Validate_Should_Throw_On_Duplicate_Command_Name()
    {
        var definitions = new[]
        {
            new CommandDefinition("a", "helm", ["one"], []),
            new CommandDefinition("a", "helm", ["two"], [])
        };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Duplicate command name");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Empty_Verb_List()
    {
        var definitions = new[] { new CommandDefinition("a", "helm", [], []) };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("no verb phrases");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Duplicate_Verb_In_Same_Command()
    {
        var definitions = new[]
        {
            new CommandDefinition("a", "helm", ["heading", "heading"], [])
        };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("duplicate verb phrase");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Duplicate_Phrase_In_Same_Context()
    {
        var definitions = new[]
        {
            new CommandDefinition("a", "helm", ["course"], []),
            new CommandDefinition("b", "helm", ["course"], [])
        };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("verb phrase 'course'");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Unknown_Parser_Key()
    {
        var definitions = new[]
        {
            new CommandDefinition("a", "helm", ["heading"], [], "missing")
        };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions, ["other"]))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("unknown argument parser key");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Empty_Verb_Phrase()
    {
        var definitions = new[] { new CommandDefinition("a", "helm", ["  "], []) };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("empty verb phrase");
    }

    [Test]
    public async Task Validate_Should_Throw_On_Cross_Context_Phrase_Overlap()
    {
        var definitions = new[]
        {
            new CommandDefinition("a", "helm", ["scan"], []),
            new CommandDefinition("b", "tactical", ["scan"], [])
        };

        await Assert.That(() => CommandRegistryValidator.Validate(definitions, failOnCrossContextPhraseOverlap: true))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("multiple contexts");
    }

    [Test]
    public async Task ValidateArgumentParserKeys_NullArguments_Throw()
    {
        var definitions = new[] { new CommandDefinition("a", "helm", ["scan"], []) };

        await Assert.That(() => CommandRegistryValidator.ValidateArgumentParserKeys(null!, ["scan"]))
            .Throws<ArgumentNullException>();
        await Assert.That(() => CommandRegistryValidator.ValidateArgumentParserKeys(definitions, null!))
            .Throws<ArgumentNullException>();
    }
}
