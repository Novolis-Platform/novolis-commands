using Microsoft.Extensions.DependencyInjection;
using Novolis.Commands.Abstractions;
using Novolis.Commands.DependencyInjection;
using Novolis.Commands.Engine;
using Novolis.Commands.Queueing;

namespace Novolis.Commands.Unit;

public sealed class CommandsDependencyInjectionTests
{
    private sealed class Ctx;

    private sealed class Resolver : ICommandContextResolver<Ctx>
    {
        public IReadOnlyDictionary<string, string> GetContextAliases(Ctx context) =>
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> GetVerbAliases(Ctx context) =>
            new Dictionary<string, string>();
    }

    [Test]
    public async Task AddNovolisCommands_RegistersEngineAndQueue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandContextResolver<Ctx>, Resolver>();
        services.AddNovolisCommands<Ctx>();

        await using var sp = services.BuildServiceProvider();
        await Assert.That(sp.GetService<ICommandRegistry>()).IsNotNull();
        await Assert.That(sp.GetService<ICommandQueue>()).IsNotNull();
        await Assert.That(sp.GetService<ICommandEngine<Ctx>>()).IsNotNull();
        await Assert.That(sp.GetService<BuiltInCommandMatcher>()).IsNotNull();
    }

    [Test]
    public async Task AddNovolisCommands_NullServices_Throws()
    {
        await Assert.That(() => ServiceCollectionExtensions.AddNovolisCommands<object>(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddNovolisCommands_Configures_Registry_And_Engine_Options()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandContextResolver<Ctx>, Resolver>();
        services.AddNovolisCommands<Ctx>(
            configureRegistry: b => b.Add("test.echo", "lab", ["echo"], "echo"),
            configureEngine: o => o.ArgumentParsers.Register("echo", new EchoParser()));

        await using var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ICommandRegistry>();
        await Assert.That(registry.GetAll().Any(d => d.Name == "test.echo")).IsTrue();

        var engine = sp.GetRequiredService<ICommandEngine<Ctx>>();
        var result = await engine.ParseCommandAsync("lab echo hi", new Ctx());
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Arguments["text"]).IsEqualTo("hi");
    }

    [Test]
    public async Task AddNovolisCommandRunner_Registers_Runner()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandQueue, ChannelCommandQueue>();
        services.AddSingleton<ICommandProcessor<Ctx>, NoOpProcessor>();
        services.AddNovolisCommandRunner<Ctx>();

        await using var sp = services.BuildServiceProvider();
        await Assert.That(sp.GetService<CommandQueueRunner<Ctx>>()).IsNotNull();
    }

    [Test]
    public async Task AddNovolisCommandRunner_NullServices_Throws()
    {
        await Assert.That(() => ServiceCollectionExtensions.AddNovolisCommandRunner<object>(null!))
            .Throws<ArgumentNullException>();
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
            arguments = new Dictionary<string, object?> { ["text"] = string.Join(" ", argumentTokens) };
            return true;
        }
    }

    private sealed class NoOpProcessor : ICommandProcessor<Ctx>
    {
        public ValueTask ProcessAsync(CommandEnvelope command, Ctx context, CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;
    }
}
